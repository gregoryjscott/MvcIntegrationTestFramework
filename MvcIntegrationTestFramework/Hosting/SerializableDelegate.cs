// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SerializableDelegate.cs" company="Public">
//   Free
// </copyright>
// <summary>
//   Makes delegates serializable where possible
//   Used to pass test delegates from the test appdomain into the ASP.NET host appdomain
//   Adapted from http://www.codeproject.com/KB/cs/AnonymousSerialization.aspx
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MvcIntegrationTestFramework.Hosting
{
    using System;
    using System.Reflection;
    using System.Runtime.Serialization;

    /// <summary>
    /// Makes delegates serializable where possible
    /// Used to pass test delegates from the test appdomain into the ASP.NET host appdomain
    /// Adapted from http://www.codeproject.com/KB/cs/AnonymousSerialization.aspx
    /// </summary>
    /// <typeparam name="TDelegate">The type of the delegate.</typeparam>
    [Serializable]
    internal class SerializableDelegate<TDelegate> : ISerializable
        where TDelegate : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableDelegate{TDelegate}"/> class.
        /// </summary>
        /// <param name="delegate">
        /// The delegate.
        /// </param>
        internal SerializableDelegate(TDelegate @delegate)
        {
            this.Delegate = @delegate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableDelegate{TDelegate}"/> class.
        /// </summary>
        /// <param name="info">
        /// The info.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        internal SerializableDelegate(SerializationInfo info, StreamingContext context)
        {
            var delegateType = (Type)info.GetValue("delegateType", typeof(Type));

            if (info.GetBoolean("isSerializable"))
            {
                // If it's a "simple" delegate we just read it straight off
                this.Delegate = (TDelegate)info.GetValue("delegate", delegateType);
            }
            else
            {
                // otherwise, we need to read its anonymous class
                var methodInfo = (MethodInfo)info.GetValue("method", typeof(MethodInfo));
                var anonymousClassWrapper = (AnonymousClassWrapper)info.GetValue("class", typeof(AnonymousClassWrapper));
                this.Delegate =
                    (TDelegate)
                    (object)
                    System.Delegate.CreateDelegate(delegateType, anonymousClassWrapper.TargetInstance, methodInfo);
            }
        }

        /// <summary>
        /// Gets Delegate.
        /// </summary>
        public TDelegate Delegate { get; private set; }

        /// <summary>
        /// The get object data.
        /// </summary>
        /// <param name="info">
        /// The info.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("delegateType", this.Delegate.GetType());
            var untypedDelegate = (Delegate)(object)this.Delegate;

            // If it's an "simple" delegate we can serialize it directly
            if ((untypedDelegate.Target == null
                 ||
                 untypedDelegate.Method.DeclaringType.GetCustomAttributes(typeof(SerializableAttribute), false).Length
                 > 0) && this.Delegate != null)
            {
                info.AddValue("isSerializable", true);
                info.AddValue("delegate", this.Delegate);
            }
            else
            {
                // otherwise, serialize anonymous class
                info.AddValue("isSerializable", false);
                info.AddValue("method", untypedDelegate.Method);
                info.AddValue(
                    "class", new AnonymousClassWrapper(untypedDelegate.Method.DeclaringType, untypedDelegate.Target));
            }
        }

        /// <summary>
        /// The anonymous class wrapper.
        /// </summary>
        [Serializable]
        private class AnonymousClassWrapper : ISerializable
        {
            /// <summary>
            /// The target type.
            /// </summary>
            private readonly Type targetType;

            /// <summary>
            /// Initializes a new instance of the <see cref="AnonymousClassWrapper"/> class.
            /// </summary>
            /// <param name="targetType">
            /// The target type.
            /// </param>
            /// <param name="targetInstance">
            /// The target instance.
            /// </param>
            internal AnonymousClassWrapper(Type targetType, object targetInstance)
            {
                this.targetType = targetType;
                this.TargetInstance = targetInstance;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="AnonymousClassWrapper"/> class.
            /// </summary>
            /// <param name="info">
            /// The serialization info.
            /// </param>
            /// <param name="context">
            /// The streaming context.
            /// </param>
            internal AnonymousClassWrapper(SerializationInfo info, StreamingContext context)
            {
                var classType = (Type)info.GetValue("classType", typeof(Type));
                this.TargetInstance = Activator.CreateInstance(classType);

                foreach (var field in classType.GetFields())
                {
                    if (typeof(Delegate).IsAssignableFrom(field.FieldType))
                    {
                        // The field is a delegate
                        field.SetValue(
                            this.TargetInstance,
                            ((SerializableDelegate<TDelegate>)info.GetValue(field.Name, typeof(SerializableDelegate<TDelegate>))).Delegate);
                    }
                    else if (!field.FieldType.IsSerializable)
                    {
                        // If the field is an anonymous class
                        field.SetValue(
                            this.TargetInstance,
                            ((AnonymousClassWrapper)info.GetValue(field.Name, typeof(AnonymousClassWrapper))).TargetInstance);
                    }
                    else
                    {
                        // otherwise
                        field.SetValue(this.TargetInstance, info.GetValue(field.Name, field.FieldType));
                    }
                }
            }

            /// <summary>
            /// Gets TargetInstance.
            /// </summary>
            public object TargetInstance { get; private set; }

            /// <summary>
            /// The get object data.
            /// </summary>
            /// <param name="info">
            /// The info.
            /// </param>
            /// <param name="context">
            /// The context.
            /// </param>
            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("classType", this.targetType);

                foreach (var field in this.targetType.GetFields())
                {
                    // See corresponding comments above
                    if (typeof(Delegate).IsAssignableFrom(field.FieldType))
                    {
                        info.AddValue(
                            field.Name, 
                            new SerializableDelegate<TDelegate>((TDelegate)field.GetValue(this.TargetInstance)));
                    }
                    else if (!field.FieldType.IsSerializable)
                    {
                        info.AddValue(
                            field.Name, new AnonymousClassWrapper(field.FieldType, field.GetValue(this.TargetInstance)));
                    }
                    else
                    {
                        info.AddValue(field.Name, field.GetValue(this.TargetInstance));
                    }
                }
            }
        }
    }
}