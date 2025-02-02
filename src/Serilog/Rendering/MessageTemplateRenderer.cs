// Copyright 2013-2017 Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Serilog.Rendering;

static class MessageTemplateRenderer
{
    static readonly JsonValueFormatter JsonValueFormatter = new("$type");
#if !NET35 && !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static void Render(MessageTemplate messageTemplate, 
#if NET35 || NET40
    IDictionary
#else
    IReadOnlyDictionary
#endif
        <string, LogEventPropertyValue> properties, TextWriter output, string? format = null, IFormatProvider? formatProvider = null)
    {
        bool isLiteral = false, isJson = false;

        if (format != null)
        {
            for (var i = 0; i < format.Length; ++i)
            {
                if (format[i] == 'l')
                    isLiteral = true;
                else if (format[i] == 'j')
                    isJson = true;
            }
        }

        for (var ti = 0; ti < messageTemplate.TokenArray.Length; ++ti)
        {
            var token = messageTemplate.TokenArray[ti];
            if (token is TextToken tt)
            {
                RenderTextToken(tt, output);
            }
            else
            {
                var pt = (PropertyToken)token;
                RenderPropertyToken(pt, properties, output, formatProvider, isLiteral, isJson);
            }
        }
    }

    public static void RenderTextToken(TextToken tt, TextWriter output)
    {
        output.Write(tt.Text);
    }

    public static void RenderPropertyToken(PropertyToken pt, 
#if NET35 || NET40
    IDictionary
#else
    IReadOnlyDictionary
#endif
        <string, LogEventPropertyValue> properties, TextWriter output, IFormatProvider? formatProvider, bool isLiteral, bool isJson)
    {
        if (!properties.TryGetValue(pt.PropertyName, out var propertyValue))
        {
            output.Write(pt.RawText);
            return;
        }

        if (!pt.Alignment.HasValue)
        {
            RenderValue(propertyValue, isLiteral, isJson, output, pt.Format, formatProvider);
            return;
        }

        var valueOutput = new StringWriter();
        RenderValue(propertyValue, isLiteral, isJson, valueOutput, pt.Format, formatProvider);
        var value = valueOutput.ToString();

        if (value.Length >= pt.Alignment.Value.Width)
        {
            output.Write(value);
            return;
        }

        Padding.Apply(output, value, pt.Alignment.Value);
    }

    static void RenderValue(LogEventPropertyValue propertyValue, bool literal, bool json, TextWriter output, string? format, IFormatProvider? formatProvider)
    {
        if (literal && propertyValue is ScalarValue {Value: string str})
        {
            output.Write(str);
        }
        else if (json && format == null)
        {
            JsonValueFormatter.Format(propertyValue, output);
        }
        else
        {
            propertyValue.Render(output, format, formatProvider);
        }
    }
}
