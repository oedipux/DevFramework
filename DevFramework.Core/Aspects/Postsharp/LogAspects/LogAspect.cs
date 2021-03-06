﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PostSharp.Aspects;
using DevFramework.Core.CrossCuttingConcerns.Logging.Log4Net;
using System.Reflection;
using DevFramework.Core.CrossCuttingConcerns.Logging;
using PostSharp.Extensibility;

namespace DevFramework.Core.Aspects.Postsharp.LogAspects
{
    [Serializable]
    [MulticastAttributeUsage(MulticastTargets.Method, TargetMemberAttributes = MulticastAttributes.Instance)]
    public class LogAspect : OnMethodBoundaryAspect
    {
        private LoggerService _loggerService;
        private Type _loggerType;

        public LogAspect(Type loggerType)
        {
            _loggerType = loggerType;
        }

        public override void RuntimeInitialize(MethodBase method)
        {

            if (_loggerType.BaseType != typeof(LoggerService))
                throw new Exception($"Invalid Logger Type: " + _loggerType.Name);

            _loggerService = (LoggerService)Activator.CreateInstance(_loggerType);

            base.RuntimeInitialize(method);
        }

        public override void OnEntry(MethodExecutionArgs args)
        {
            if (!_loggerService.IsInfoEnable)
                return;

            try
            {

                var logParameters = args.Method.GetParameters().Select((t, i) => new LogParameter()
                {
                    Name = t.Name,
                    Type = t.ParameterType.Name,
                    Value = args.Arguments.GetArgument(i)
                }).ToList();

                var logDetail = new LogDetail()
                {
                    FullName = args.Method.DeclaringType == null ? null : args.Method.DeclaringType.Name,
                    MethodName = args.Method.Name,
                    Parameters = logParameters
                };

                _loggerService.Info(logDetail);

            }
            catch
            {
                //ignored
            }

            base.OnEntry(args);
        }
    }
}
