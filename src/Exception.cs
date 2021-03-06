﻿#region SearchAThing.Core, Copyright(C) 2015-2016 Lorenzo Delana, License under MIT
/*
* The MIT License(MIT)
* Copyright(c) 2016 Lorenzo Delana, https://searchathing.com
*
* Permission is hereby granted, free of charge, to any person obtaining a
* copy of this software and associated documentation files (the "Software"),
* to deal in the Software without restriction, including without limitation
* the rights to use, copy, modify, merge, publish, distribute, sublicense,
* and/or sell copies of the Software, and to permit persons to whom the
* Software is furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
* FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
* DEALINGS IN THE SOFTWARE.
*/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using static System.Math;

namespace SearchAThing
{

    public class ErrorInfo
    {

        public string Message { get; set; }
        public string ExceptionType { get; set; }
        public string Stacktrace { get; set; }
        public string InnerException { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"exception message : [{Message}]");
            sb.AppendLine($"exception type : [{ExceptionType}]");
            sb.AppendLine($"stacktrace : [{Stacktrace}]");

            return sb.ToString();
        }

    }

    public static partial class Extensions
    {

        public static string Details(this Exception ex)
        {
            return DetailsObject(ex).ToString();
        }

        public static ErrorInfo DetailsObject(this Exception ex)
        {
            var res = new ErrorInfo();

            try
            {
                res.Message = ex.Message;
                res.ExceptionType = ex.GetType().ToString();
                res.Stacktrace = ex.StackTrace.ToString();

                var sb = new StringBuilder();

                Func<Exception, string> inner_detail = null;
                inner_detail = (e) =>
                {
                    if (e is Npgsql.PostgresException)
                    {
                        var pex = e as Npgsql.PostgresException;

                        sb.AppendLine($"npgsql statement [{pex.Statement}]");
                    }

                    if (e is System.Data.Entity.Validation.DbEntityValidationException)
                    {
                        var dex = e as System.Data.Entity.Validation.DbEntityValidationException;

                        foreach (var deve in dex.EntityValidationErrors)
                        {
                            sb.Append($"db validation error entry : [{deve.Entry}]");

                            foreach (var k in deve.ValidationErrors)
                            {
                                sb.Append($" {k.ErrorMessage}");
                            }
                            sb.AppendLine();
                        }
                    }

                    if (e.InnerException != null)
                    {
                        sb.AppendLine($"inner exception : {e.InnerException.Message}");

                        inner_detail(e.InnerException);
                    }
                    return "";
                };

                inner_detail(ex);

                if (sb.Length > 0)
                    res.InnerException = sb.ToString();

                return res;
            }
            catch (Exception ex0)
            {
                res.Message = $"exception generating ex detail : {ex0?.Message}";
            }

            return res;
        }

    }

}

