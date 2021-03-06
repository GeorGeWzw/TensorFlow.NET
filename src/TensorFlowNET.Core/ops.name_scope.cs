﻿/*****************************************************************************
   Copyright 2018 The TensorFlow.NET Authors. All Rights Reserved.

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
******************************************************************************/

using System.Collections.Generic;
using System.Diagnostics;
using Tensorflow.Eager;
using static Tensorflow.Binding;

namespace Tensorflow
{
    public partial class ops
    {
        public static NameScope name_scope(string name,
            string default_name = "",
            object values = null) => new NameScope(name, default_name, values);

        /// <summary>
        /// Returns a context manager that creates hierarchical names for operations.
        /// </summary>
        public class NameScope : ITensorFlowObject
        {
            public string _name;
            public string _default_name;
            public object _values;
            public string scope_name;
            public string old_scope_name = "";
            
            public NameScope(string name, string default_name = "", object values = null)
            {
                _name = name;
                _default_name = default_name;
                _values = values;
            }

            public void __enter__()
            {
                _name = _name ?? _default_name;
                if (tf.context.executing_eagerly())
                {
                    (scope_name, old_scope_name) = enter_eager_name_scope(tf.context, _name);
                }
                else
                {
                    Graph g = null;

                    if (_values is List<Tensor> vList)
                        g = _get_graph_from_inputs(vList.ToArray());
                    else if (_values is Tensor[] vArray)
                        g = _get_graph_from_inputs(vArray);

                    if (g == null)
                        g = get_default_graph();

                    old_scope_name = g._name_stack;
                    scope_name = g.name_scope(_name);
                }
            }

            private (string, string) enter_eager_name_scope(Context ctx, string name)
            {
                if (name == null)
                    name = "";

                var scope_name = name;
                var old_name = ctx.scope_name;
                // A trailing slash breaks out of nested name scopes, indicating a
                // fully specified scope name, for compatibility with Graph.name_scope.
                if (!name.EndsWith("/"))
                {
                    scope_name = name + "/";
                    if (!string.IsNullOrEmpty(old_name))
                        scope_name = old_name + scope_name;
                }

                ctx.scope_name = scope_name;
                return (scope_name, old_name);
            }

            public void Dispose()
            {
                if (tf.context.executing_eagerly())
                    tf.context.scope_name = old_scope_name;
                else
                    get_default_graph()._name_stack = old_scope_name;
            }
            
            [DebuggerNonUserCode]
            public void __exit__()
            {
            }

            [DebuggerNonUserCode]
            public void __init__()
            {
                
            }

            [DebuggerNonUserCode]
            public void __del__()
            {
                
            }
            
            /// <summary>
            /// __enter__()
            /// </summary>
            /// <param name="ns"></param>
            public static implicit operator string(NameScope ns)
            {
                return ns.scope_name;
            }
        }
    }
}
