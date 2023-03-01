﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using Microsoft.PowerFx.Core.IR;
using Microsoft.PowerFx.Types;

namespace Microsoft.PowerFx.Types
{
    public class VoidValue : ValidFormulaValue
    {
        internal VoidValue(IRContext irContext) 
            : base(irContext)
        {
            Contract.Assert(irContext.ResultType == FormulaType.Void);
        }

        public override void ToExpression(StringBuilder sb, FormulaValueSerializerSettings settings)
        {
            sb.Append($"If(true, {{test:1}}, \"Mismatched args\")");
        }

        public override object ToObject()
        {
            return null;
        }

        public override string ToString()
        {
            return "-";
        }

        public override void Visit(IValueVisitor visitor)
        {
            throw new NotSupportedException();
        }
    }
}
