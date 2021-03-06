﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Xigadee
{
    /// <summary>
    /// This class is used to marshall the method signature
    /// </summary>
    [DebuggerDisplay("{Method.Name}")]
    public class CommandMethodSignature
    {
        #region Constructor
        /// <summary>
        /// This is the default constructor.
        /// </summary>
        /// <param name="command">The command to process.</param>
        /// <param name="method">The method for the signature.</param>
        /// <param name="throwSignatureException">This property specifies whether the system should throw exceptions if a signature is invalid.</param>
        public CommandMethodSignature(ICommand command, MethodInfo method, bool throwSignatureException = false)
        {
            if (method == null)
                throw new CommandMethodSignatureException($"Constructor error - info cannot be null.");

            if (command == null)
                throw new CommandMethodSignatureException($"Constructor error - command cannot be null.");

            Command = command;

            Method = method;

            IsValid = Validate(throwSignatureException);
        }
        #endregion

        /// <summary>
        /// This is the command connected to the signature
        /// </summary>
        public ICommand Command { get; }
        /// <summary>
        /// This is the specific method.
        /// </summary>
        public MethodInfo Method { get; }
        /// <summary>
        /// Specifies whether this is a generic signature.
        /// </summary>
        public bool IsStandardCall { get; private set; }
        /// <summary>
        /// This specifies whether the method call is async.
        /// </summary>
        public bool IsAsync { get; private set; }
        /// <summary>
        /// this property specifies whether the return value is the response parameter.
        /// </summary>
        public bool IsReturnValue { get; private set; }
        /// <summary>
        /// This property specifies whether the signature is valid.
        /// </summary>
        public bool IsValid { get; }
        /// <summary>
        /// These are the assigned command attributes.
        /// </summary>
        public List<CommandContractAttribute> CommandAttributes { get; protected set; }
        /// <summary>
        /// This is the list of the parameters for the method.
        /// </summary>
        public List<ParameterInfo> Parameters { get; protected set; }
        /// <summary>
        /// This method validates the method.
        /// </summary>
        /// <param name="throwException">Set this to true throw an exception if the signature does not match,</param>
        /// <returns>Returns true if the signature is validated.</returns>
        protected virtual bool Validate(bool throwException = false)
        {
            try
            {
                CommandAttributes = Attribute.GetCustomAttributes(Method)
                    .Where((a) => a is CommandContractAttribute)
                    .Cast<CommandContractAttribute>()
                    .ToList();

                //This shouldn't happen, but check anyway.
                if (CommandAttributes.Count == 0)
                    if (throwException)
                        throw new CommandMethodSignatureException($"CommandAttributes are not defined for the method.");
                    else
                        return false;

                //OK, check whether the return parameter is a Task or Task<> async construct
                IsAsync = typeof(Task).IsAssignableFrom(Method.ReturnParameter.ParameterType);

                Parameters = Method.GetParameters().ToList();
                var paramInfo = Method.GetParameters().ToList();

                //OK, see if the standard parameters exist and aren't decorated as In or Out.
                StandardIn = paramInfo
                    .Where((p) => !ParamAttributes<PayloadInAttribute>(p))
                    .Where((p) => !ParamAttributes<PayloadOutAttribute>(p))
                    .FirstOrDefault((p) => p.ParameterType == typeof(TransmissionPayload));
                bool isStandardIn = (StandardIn != null) && paramInfo.Remove(StandardIn);
                if (StandardIn != null)
                    StandardInPos = Parameters.IndexOf(StandardIn);

                StandardOut = paramInfo
                    .Where((p) => !ParamAttributes<PayloadInAttribute>(p))
                    .Where((p) => !ParamAttributes<PayloadOutAttribute>(p))
                    .FirstOrDefault((p) => p.ParameterType == typeof(List<TransmissionPayload>));
                bool isStandardOut = (StandardOut != null) && paramInfo.Remove(StandardOut);
                if (StandardOut != null)
                    StandardOutPos = Parameters.IndexOf(StandardOut);

                IsStandardCall = (isStandardIn || isStandardOut) && paramInfo.Count == 0;

                if (IsStandardCall)
                    return true;

                //Get the In parameter
                ParamIn = Parameters.Where((p) => ParamAttributes<PayloadInAttribute>(p)).FirstOrDefault();
                if (ParamIn != null && paramInfo.Remove(ParamIn))
                {
                    TypeIn = ParamIn?.ParameterType;
                    ParamInPos = Parameters.IndexOf(ParamIn);
                }

                //Now get the out parameter
                ParamOut = Parameters.Where((p) => ParamAttributes<PayloadOutAttribute>(p)).FirstOrDefault();

                if (ParamOut == null && ParamAttributes<PayloadOutAttribute>(Method.ReturnParameter))
                {
                    ParamOut = Method.ReturnParameter;
                    IsReturnValue = true;
                }
                else if (ParamOut != null && paramInfo.Remove(ParamOut))
                {
                    ParamOutPos = Parameters.IndexOf(ParamOut);
                }

                if (ParamOut != null && !IsReturnValue && !ParamOut.IsOut)
                    if (throwException)
                        throw new CommandMethodSignatureException($"Parameter {ParamOut.Name} is not marked as an out parameter.");
                    else
                        return false;

                if (IsAsync && IsReturnValue && ParamOut.ParameterType.IsGenericType)
                {
                    if (ParamOut.ParameterType.GenericTypeArguments.Length != 1)
                        if (throwException)
                            throw new CommandMethodSignatureException($"Generic Task response parameter can only have one parameter.");
                        else
                            return false;

                    TypeOut = ParamOut.ParameterType.GenericTypeArguments[0];
                }
                else if (!IsAsync)
                {
                    TypeOut = ParamOut.ParameterType;
                }

                //Finally check that we have used all the parameters.
                if (paramInfo.Count != 0 && throwException)
                    throw new CommandMethodSignatureException($"There are too many parameters in the method ({paramInfo[0].Name}).");

                return paramInfo.Count == 0;

            }
            catch (Exception ex)
            {
                throw new CommandMethodSignatureException("PayloadIn or PayloadOut have not been set correctly.") { InnerException = ex };
            }
        }

        private bool ParamAttributes<A>(ParameterInfo info)
            where A : Attribute
        {
            try
            {
                var attr = Attribute.GetCustomAttribute(info, typeof(A), false);

                return attr != null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public ParameterInfo StandardIn { get; private set; }
        public int? StandardInPos { get; private set; }

        public ParameterInfo StandardOut { get; private set; }
        public int? StandardOutPos { get; private set; }

        public ParameterInfo ParamIn { get; private set; }
        public int? ParamInPos { get; private set; }

        public Type TypeIn { get; set; }

        public ParameterInfo ParamOut { get; private set; }
        public int? ParamOutPos { get; private set; }

        public Type TypeOut { get; set; }

        #region Reference(CommandContractAttribute attr)
        /// <summary>
        /// This is the command reference.
        /// </summary>
        /// <param name="attr">The contract attribute.</param>
        /// <returns>The reference id.</returns>
        public string Reference(CommandContractAttribute attr)
        {
            return $"{Method.Name}/{attr.Header.ToKey()}";
        }
        #endregion

        /// <summary>
        /// This is the command action that is executed.
        /// </summary>
        public Func<TransmissionPayload, List<TransmissionPayload>, IPayloadSerializationContainer, Task> Action
        {
            get
            {
                return async (pIn, pOut, ser) =>
                {
                    try
                    {
                        var collection = new object[Parameters.Count];

                        if (ParamInPos.HasValue)
                            collection[ParamInPos.Value] = ser.PayloadDeserialize(pIn.Message);

                        if (StandardInPos.HasValue)
                            collection[StandardInPos.Value] = pIn;

                        if (StandardOutPos.HasValue)
                            collection[StandardOutPos.Value] = pOut;

                        object output = null;

                        if (IsAsync)
                        {
                            if (TypeOut == null)
                                await (Task)Method.Invoke(Command, collection);
                            else
                                output = await (dynamic)Method.Invoke(Command, collection);
                        }
                        else
                        {
                            if (!IsReturnValue)
                            {
                                Method.Invoke(Command, collection);
                                if (ParamOutPos.HasValue)
                                    output = collection[ParamOutPos.Value];
                            }
                            else
                                output = (dynamic)Method.Invoke(Command, collection);
                        }

                        if (TypeOut != null)
                        {
                            var response = pIn.ToResponse();
                            response.Message.Blob = ser.PayloadSerialize(output);
                            response.Message.Status = "200";

                            pOut.Add(response);
                        }
                    }
                    catch (Exception ex)
                    {

                        throw;
                    }

                };
            }
        }
    }
}