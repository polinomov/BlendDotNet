using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace blend
{
    class Mixer
    {

        static AssemblyDefinition Fille2Assm(string assmPath, string searchDir)
        {
            var resolver = new DefaultAssemblyResolver();
            if(searchDir != null) resolver.AddSearchDirectory(searchDir);
            var readerParameters = new ReaderParameters
            {
                ReadSymbols = true,
                ReadWrite = true,
                SymbolReaderProvider = new DefaultSymbolReaderProvider(false),
                AssemblyResolver = resolver
            };
            AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(assmPath, readerParameters);
            return asm;
        }

        static void Assm2File(AssemblyDefinition assm, string fPath)
        {
            var writerParameters = new WriterParameters()
            {
                WriteSymbols = assm.MainModule.HasSymbols
            };
            assm.Write(fPath,writerParameters);
        }

        static void CopyInstruction(Instruction ins, MethodDefinition method, ILProcessor proc, TypeDefinition typeSrc)
        {
            if (ins.Operand != null)
            {
                var tp = ins.Operand.GetType();
                if (tp == typeof(Mono.Cecil.MethodReference))
                {
                    var callMeRef = method.Module.ImportReference((Mono.Cecil.MethodReference)ins.Operand);
                    //Console.WriteLine(ins.Operand.ToString());
                    proc.Emit(ins.OpCode, callMeRef);
                }
                else if (tp == typeof(Mono.Cecil.Cil.Instruction))
                {
                    proc.Emit(ins.OpCode, (Mono.Cecil.Cil.Instruction)ins.Operand);
                }
                else if (tp == typeof(FieldDefinition))
                {
                    FieldDefinition fd = ins.Operand as FieldDefinition;
                    FieldDefinition toAdd = typeSrc.Fields.FirstOrDefault(a => a.FullName == fd.FullName);
                    proc.Emit(ins.OpCode, toAdd);
                }
                else if (tp == typeof(System.SByte))
                {
                    proc.Emit(ins.OpCode, (System.SByte)ins.Operand);
                }
                else if (tp == typeof(System.String))
                {
                    proc.Emit(ins.OpCode, (System.String)ins.Operand);
                }
                else
                {
                    Console.WriteLine("Unknown  operand" + tp);
                }
           }
            else
            {
               proc.Emit(ins.OpCode);
            }
        }

        static void CopyVars(MethodDefinition methodSrc, MethodDefinition methodDst)
        {
            foreach (var v in methodSrc.Body.Variables) methodDst.Body.Variables.Add(v);
        }

        static void CopyMembers(TypeDefinition typeSrc, TypeDefinition typeDst)
        {
            foreach (var v in typeSrc.Fields)
            {
               //Console.WriteLine(v.ToString());
                FieldDefinition fd = new FieldDefinition(v.Name, v.Attributes, v.FieldType);
                
                //FieldDefinition fd = new FieldDefinition("XREN", v.Attributes, v.FieldType);
                typeDst.Fields.Add(fd);
            }
        }

        static void AssmScan(AssemblyDefinition assm, AssemblyDefinition assmPrime)
        {
            var primeModyle = assmPrime.MainModule;
            var module = assm.MainModule;

            var references = module.AssemblyReferences;

            foreach (var type in assm.MainModule.GetTypes())
            {
                if (primeModyle.GetType(type.FullName) == null)
                {
                    TypeDefinition typeToAdd = new TypeDefinition(type.Namespace,type.Name, type.Attributes,type.BaseType);
                    primeModyle.Types.Add(typeToAdd);
                    CopyMembers(type, typeToAdd);

                    foreach (var method in type.Methods)
                    {
                        MethodDefinition mdef = new MethodDefinition(method.Name, method.Attributes, method.ReturnType);
                        foreach (var p in method.Parameters) mdef.Parameters.Add(p);
                        typeToAdd.Methods.Add(mdef);
                        var ilProcessor = mdef.Body.GetILProcessor();
                        foreach (Instruction ins in method.Body.Instructions)
                        {
                            CopyInstruction(ins, mdef, mdef.Body.GetILProcessor(), typeToAdd);
                        }
                        CopyVars(method, mdef);
                    }
                }
            }
        }

        static void ModifyMethodInstr(MethodDefinition method, int ndx,Instruction i, Instruction trg)
        {
            ILProcessor iproc = method.Body.GetILProcessor();
            iproc.Append(Instruction.Create(OpCodes.Nop));
            iproc.Replace(trg, i);
            // iproc.Append(i);
            //iproc.InsertAfter(ndx, i);
        }

        static void FixRefs(AssemblyDefinition assm)
        {
            foreach (var type in assm.MainModule.GetTypes())
            {
               foreach (var method in type.Methods)
               {
                    if (method.HasBody == false) continue;
                    List<Instruction> insertMe = new List<Instruction>();
                    List<Instruction> trg = new List<Instruction>();
                    List<Tuple<Instruction, Instruction>> lst = new List<Tuple<Instruction, Instruction>>();
                       int ni =-1,cnt = 0;
                    foreach (Instruction ins in method.Body.Instructions)
                    {
 
                        if (ins.OpCode.Code == Code.Call)
                        {
                            if (ins.Operand.GetType() == typeof(MethodDefinition))   
                            {
                                Console.WriteLine("--" + ins.Operand.GetType());
                            }
                        }

                        if ((ins.Operand != null)  && (ins.Operand.GetType() == typeof(Mono.Cecil.MethodReference)))
                        {

                            MethodReference mr = ins.Operand as MethodReference;
                            TypeDefinition tt =assm.MainModule.GetType(mr.DeclaringType.FullName);
                            if (tt != null)
                            {
                                var theM = tt.Methods.FirstOrDefault(a => a.FullName == mr.FullName);
                                //Console.WriteLine(mr.DeclaringType.FullName);
                                //Console.WriteLine(mr.FullName);
                                insertMe.Add(Instruction.Create(OpCodes.Call, theM));
                                ni = cnt;
                                trg.Add(ins);
                                var callIns = Instruction.Create(OpCodes.Call, theM);
                                Tuple<Instruction, Instruction> replacePair = new Tuple<Instruction, Instruction>(ins, callIns);
                                lst.Add(replacePair);


                                //iproc.Emit(i.OpCode, (MethodDefinition)i.Operand);
                                Console.WriteLine("Replaicing "  + ins.ToString() + "->" + insertMe.ToString() );
                            
                                //method.Body.Instructions.Insert(1, Instruction.Create(OpCodes.Call, theM));
                                // method.Module.ImportReference
                                // MethodDefinition mdef = new MethodDefinition
                            }

                            // var r = assm.MainModule.ImportReference(mr.DeclaringType);
                            // assm.MainModule.GetTypes()

                            // Console.WriteLine(ins.Operand.ToString());

                            // method.Module.ImportReference(typeof(InjectorRt).GetMethod("CallMe"));
                        }
                        cnt++;
                    }// end instructions

                    ILProcessor iproc = method.Body.GetILProcessor();
                    foreach (var rep in lst)
                    {
                        iproc.Replace(rep.Item1, rep.Item2);
                    }

                    /*
                    if (insertMe != null)
                    {
                        ModifyMethodInstr(method, ni ,insertMe,trg);
                    }
                    */

                }//end method
            }
        }

        public static void DoMix(string primeAssmPath, List<string> assmList, string result)
        {
            AssemblyDefinition assmPrime = Fille2Assm(primeAssmPath, null);
            var references = assmPrime.MainModule.AssemblyReferences;

            foreach (string st in assmList)
            {
                AssemblyDefinition assmDef = Fille2Assm(st, null);
                AssmScan(assmDef, assmPrime);
            }

            FixRefs(assmPrime);
            Assm2File(assmPrime, result);
        }

    }
}
