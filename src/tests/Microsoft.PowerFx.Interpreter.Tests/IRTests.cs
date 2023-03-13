﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.PowerFx.Core;
using Microsoft.PowerFx.Core.Entities;
using Microsoft.PowerFx.Core.IR;
using Microsoft.PowerFx.Core.Tests;
using Microsoft.PowerFx.Core.Types;
using Microsoft.PowerFx.Core.Utils;
using Microsoft.PowerFx.Types;
using Xunit;

namespace Microsoft.PowerFx.Interpreter.Tests
{
    public class IRTests
    {
        [Fact]
        public void ValidateNoRecordToRecordAggregateCoercion()
        {
            var tableType = TableType.Empty().Add(new NamedFormulaType(new TypedName(DType.Currency, new DName("Currency"))));

            var symbols = new SymbolTable { DebugName = "ST1 " };
            symbols.EnableMutationFunctions();
            var slot = symbols.AddVariable("MyTable", tableType);

            var engine = new RecalcEngine(new PowerFxConfig());
            var checkResult = engine.Check("Patch(MyTable, { Currency: 1.2 }, { Currency: 1.5 })", new ParserOptions() { AllowsSideEffects = true }, symbolTable: symbols);

            checkResult.ThrowOnErrors();

            var runtimeConfig = new SymbolValues(symbols) { DebugName = "SV1" };
            runtimeConfig.Set(slot, TableValue.NewTable(tableType.ToRecord()));

            var evalResult = checkResult.GetEvaluator().EvalAsync(CancellationToken.None, runtimeConfig).Result;
            Assert.IsNotType<ErrorValue>(evalResult);

            var ir = IRTranslator.Translate(checkResult.Binding).ToString();
            Assert.DoesNotContain("AggregateCoercionNode", ir);
        }

        [Theory]

        [InlineData("With({t1:Table({a:stringVar})},Patch(t1,First(t1),{a:integerVar}))")]
        [InlineData("With({t1:Table({a:5})},Patch(t1,First(t1),{a:datetimeVar}))")]
        public void RecordToRecordAggregateCoercionTest(string expr)
        {
            var engine = new RecalcEngine(new PowerFxConfig());

            var stringVar = FormulaValue.New("lichess.org");
            var integerVar = FormulaValue.New(1);
            var datetimeVar = FormulaValue.New(DateTime.Now);

            engine.Config.SymbolTable.EnableMutationFunctions();
            engine.Config.SymbolTable.AddConstant("stringVar", stringVar);
            engine.Config.SymbolTable.AddConstant("integerVar", integerVar);
            engine.Config.SymbolTable.AddConstant("datetimeVar", datetimeVar);

            var result = engine.Eval(expr, options: new ParserOptions() { AllowsSideEffects = true });

            Assert.IsNotType<ErrorValue>(result);
        }

        [Theory]
        [InlineData("[{a:0},{a:\"3\",b:2}]", "Table({a:0,b:If(false,0)},{a:3,b:2})")]
        public void RecordToRecordAggregateCoercionDontDropFieldsTest(string expr, string expected)
        {
            var iSetup = InternalSetup.Parse($"TableSyntaxDoesntWrapRecords");
            var engine = new RecalcEngine(new PowerFxConfig(Features.TableSyntaxDoesntWrapRecords));

            var result = engine.Eval(expr) as TableValue;

            Assert.Equal(expected, result.ToExpression());
        }

        private class BooleanOptionSet : OptionSet, IExternalOptionSet
        {
            public BooleanOptionSet(string name, DisplayNameProvider displayNameProvider)
                : base(name, displayNameProvider)
            {
            }

            public new bool TryGetValue(DName fieldName, out OptionSetValue optionSetValue)
            {
                if (!Options.Any(option => option.Key == fieldName))
                {
                    optionSetValue = null;
                    return false;
                }

                optionSetValue = new OptionSetValue(fieldName, FormulaType, fieldName == "1");
                return true;
            }

            DKind IExternalOptionSet.BackingKind => DKind.Boolean;
        }

        private class InvalidBooleanOptionSet : OptionSet, IExternalOptionSet
        {
            public InvalidBooleanOptionSet(string name, DisplayNameProvider displayNameProvider)
                : base(name, displayNameProvider)
            {
            }

            public new bool TryGetValue(DName fieldName, out OptionSetValue optionSetValue)
            {
                if (!Options.Any(option => option.Key == fieldName))
                {
                    optionSetValue = null;
                    return false;
                }

                // Invalid, not passing a bool value for `value`
                optionSetValue = new OptionSetValue(fieldName, FormulaType);
                return true;
            }

            DKind IExternalOptionSet.BackingKind => DKind.Boolean;
        }

        [Theory]
        [InlineData("If(BoolOptionSet.Negative, \"YES\",\"NO\")", "NO")]
        [InlineData("BoolOptionSet.Positive & \" TEXT\"", "Positive TEXT")]
        public void BooleanOptionSetTest(string expression, string expected)
        {
            var engine = new RecalcEngine(new PowerFxConfig());
            var symbol = new SymbolTable();

            var boolOptionSetDisplayNameProvider = DisplayNameUtility.MakeUnique(new Dictionary<string, string>
            {
                { "1", "Positive" },
                { "0", "Negative" },
            });

            engine.Config.AddOptionSet(new BooleanOptionSet("BoolOptionSet", boolOptionSetDisplayNameProvider));

            var check = engine.Check(expression);
            Assert.True(check.IsSuccess);

            var result = check.GetEvaluator().Eval() as StringValue;

            Assert.Equal(expected, result.Value);
        }

        [Theory]
        [InlineData("If(BoolOptionSet.Negative, \"YES\",\"NO\")")]
        public void BooleanOptionSetErrorTest(string expression)
        {
            var engine = new RecalcEngine(new PowerFxConfig());
            var symbol = new SymbolTable();

            var boolOptionSetDisplayNameProvider = DisplayNameUtility.MakeUnique(new Dictionary<string, string>
            {
                { "error", "Positive" },
                { "invalid", "Negative" },
            });

            engine.Config.AddOptionSet(new InvalidBooleanOptionSet("BoolOptionSet", boolOptionSetDisplayNameProvider));

            var check = engine.Check(expression);
            Assert.True(check.IsSuccess);

            var result = check.GetEvaluator().Eval();

            Assert.IsType<ErrorValue>(result);
            Assert.Contains("The value of option", ((ErrorValue)result).Errors.First().Message);
        }
    }
}
