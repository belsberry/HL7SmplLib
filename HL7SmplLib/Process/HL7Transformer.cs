using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.CodeDom.Compiler;
using System.Reflection;
using Microsoft.CSharp;

namespace HL7SmplLib.Process
{
    public class HL7Transformer
    {
        private string _className = "";
        private string _codeData = "";
        private string _fileName = "";
        private Assembly _compiledCode = null;
        private IHL7Processor _processor = null;

        public HL7Transformer(string className, string codeFile)
        {
            try
            {
                _className = className;
                _fileName = codeFile;
                _codeData = File.ReadAllText(codeFile);
            }
            catch (IOException ex)
            {
                throw new IOException(String.Format("Unable to load file data from file {0}. {1}", codeFile, ex.Message));
            }

            try
            {
                CompileProcessorFile();
            }
            catch (Exception ex)
            {
                throw new Exception("Error compiling code. " + ex.Message);
            }
        }

        public HL7Message TransformHL7(string message)
        {
            HL7Message hl7Message = new HL7Message(message);
            if (_processor != null)
            {
                hl7Message = _processor.ProcessHL7(hl7Message);
            }
            return hl7Message;
        }

        private void CompileProcessorFile()
        {
            CompilerParameters compileParams = new CompilerParameters();
            compileParams.GenerateInMemory = true;
            compileParams.GenerateExecutable = false;
            compileParams.TreatWarningsAsErrors = false;
            compileParams.CompilerOptions = "/optimize";

            string[] references = { "System.dll", "HL7SmplLib.dll" };
            compileParams.ReferencedAssemblies.AddRange(references);
            CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            CompilerResults results = codeProvider.CompileAssemblyFromSource(compileParams, _codeData);
            if (results.Errors.HasErrors)
            {
                //TODO do stuff
                StringBuilder sb = new StringBuilder();
                foreach (CompilerError error in results.Errors)
                {
                    sb.AppendLine(error.ErrorText);
                }
                throw new Exception("Errors in compilation: " + sb.ToString());
            }
            else
            {
                try
                {
                    _compiledCode = results.CompiledAssembly;
                    Module mod = _compiledCode.GetModules()[0];
                    if (_compiledCode != null)
                    {
                        Type classType = mod.GetType(_className);
                        _processor = (IHL7Processor)Activator.CreateInstance(classType);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Problem loading IHL7Processor Object");
                }
            }
            
        }
    }
}
