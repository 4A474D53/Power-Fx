﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Microsoft.PowerFx.Core.Localization;
using Microsoft.PowerFx.Core.Types;

namespace Microsoft.PowerFx.Core.Texl.Builtins
{
    // Int(number:n)    
    // Truncate by rounding toward negative infinity.
    internal sealed class IntFunction : MathFunction
    {
        public IntFunction()
            : base("Int", TexlStrings.AboutInt, FunctionCategories.MathAndStat, nativeDecimal: true)
        {
        }
    }

    // Int(E:*[n])
    // Table overload that applies Int to each item in the input table.
    internal sealed class IntTableFunction : MathTableFunction
    {
        public IntTableFunction()
            : base("Int", TexlStrings.AboutIntT, FunctionCategories.Table, nativeDecimal: true)
        {
        }
    }
}
