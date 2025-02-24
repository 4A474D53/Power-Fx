﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Globalization;
using System.Threading;
using Microsoft.PowerFx;
using Microsoft.PowerFx.Core.IR;
using Microsoft.PowerFx.Core.Tests;
using Microsoft.PowerFx.Interpreter;
using Microsoft.PowerFx.Types;
using Xunit;
using static Microsoft.PowerFx.Functions.Library;

namespace Microsoft.PowerFx.Tests
{
    public class LanguageTest : PowerFxTest
    {
        // Test language
        [Fact]
        public void GetLanguageTest()
        {
            var vnCulture = "vi-VN";
            var runtimeConfig = new RuntimeConfig();
            runtimeConfig.SetCulture(new CultureInfo(vnCulture));
            runtimeConfig.SetTimeZone(TimeZoneInfo.Utc);
            var runner = new EvalVisitor(runtimeConfig, CancellationToken.None);

            var language = Language(runner, IRContext.NotInSource(FormulaType.String));
            Assert.Equal(vnCulture.ToLower(), language.Value.ToLower());
        }

        // Test default language
        [Fact]
        public void GetDefaultLanguageTest()
        {
            var engine = new RecalcEngine();
            var result = engine.Eval("Language()");

            Assert.Equal("en-US", result.ToObject());
        }
    }
}
