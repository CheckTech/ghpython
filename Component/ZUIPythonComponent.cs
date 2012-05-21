﻿using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Parameters.Hints;
using System;
using System.Collections.Generic;

namespace GhPython.Component
{
    public class ZuiPythonComponent : ScriptingAncestorComponent, IGH_VariableParameterComponent
    {

        protected override void AddDefaultInput(GH_Component.GH_InputParamManager pManager)
        {
            pManager.RegisterParam(CreateParameter(GH_ParameterSide.Input, pManager.ParamCount));
            pManager.RegisterParam(CreateParameter(GH_ParameterSide.Input, pManager.ParamCount));
        }

        protected override void AddDefaultOutput(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(CreateParameter(GH_ParameterSide.Output, pManager.ParamCount));
        }

        internal override void FixGhInput(Param_ScriptVariable i, bool alsoSetIfNecessary = true)
        {
          i.Name = i.NickName;

          if (string.IsNullOrEmpty(i.Description))
              i.Description = string.Format("Script variable {0}", i.NickName);
          i.AllowTreeAccess = true;
          i.Optional = true;
          i.ShowHints = true;

          i.Hints = new List<IGH_TypeHint>();

          i.Hints.Add(PythonHints.NewMarshalling[NewDynamicAsGuidHint.ID]);
          i.Hints.AddRange(PossibleHints);

          i.Hints.RemoveAll((t) => { var y = t.GetType(); return (y == typeof(GH_DoubleHint_CS) || y == typeof(GH_StringHint_CS)); });
          i.Hints.Insert(4, PythonHints.NewMarshalling[NewFloatHint.ID]);
          i.Hints.Insert(6, PythonHints.NewMarshalling[NewStrHint.ID]);
          i.Hints.Insert(i.Hints.Count - 4, PythonHints.NewMarshalling[NewSpecialPointAsGuidHint.ID]);

          i.Hints.Add(PythonHints.GhMarshalling[typeof(GH_BoxHint)]);
          i.Hints.Add(PythonHints.NewMarshalling[NewSpecialBoxAsGuidHint.ID]);
          
          i.Hints.Add(new GH_HintSeparator());

          i.Hints.Add(PythonHints.GhMarshalling[typeof(GH_LineHint)]);
          i.Hints.Add(PythonHints.NewMarshalling[NewSpecialLineHint.ID]);

          i.Hints.Add(PythonHints.GhMarshalling[typeof(GH_CircleHint)]);
          i.Hints.Add(PythonHints.NewMarshalling[NewSpecialCircleHint.ID]);
          
          i.Hints.Add(PythonHints.GhMarshalling[typeof(GH_ArcHint)]);
          i.Hints.Add(PythonHints.NewMarshalling[NewSpecialArcAsGuidHint.ID]);

          i.Hints.Add(PythonHints.GhMarshalling[typeof(GH_PolylineHint)]);
          i.Hints.Add(PythonHints.NewMarshalling[NewSpecialPolylineHint.ID]);

          i.Hints.Add(new GH_HintSeparator());

          i.Hints.Add(PythonHints.GhMarshalling[typeof(GH_CurveHint)]);
          i.Hints.Add(PythonHints.NewMarshalling[NewSpecialCurveHint.ID]);

          i.Hints.Add(PythonHints.GhMarshalling[typeof(GH_MeshHint)]);
          i.Hints.Add(PythonHints.NewMarshalling[NewSpecialMeshHint.ID]);

          i.Hints.Add(PythonHints.GhMarshalling[typeof(GH_SurfaceHint)]);
          i.Hints.Add(PythonHints.NewMarshalling[NewSpecialSurfaceHint.ID]);

          i.Hints.Add(PythonHints.GhMarshalling[typeof(GH_BrepHint)]);
          i.Hints.Add(PythonHints.NewMarshalling[NewSpecialBrepHint.ID]);

          i.Hints.Add(PythonHints.GhMarshalling[typeof(GH_GeometryBaseHint)]);
          i.Hints.Add(PythonHints.NewMarshalling[NewSpecialGeometryBaseHint.ID]);

          if (alsoSetIfNecessary && i.TypeHint == null)
            i.TypeHint = i.Hints[0];
        }

        #region Members of IGH_VariableParameterComponent

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
          	switch (side)
	          {
		            case GH_ParameterSide.Input:
		            {
			            return new Param_ScriptVariable
			            {
				              NickName = GH_ComponentParamServer.InventUniqueNickname("xyzuvwst", this.Params.Input),
                      Name = NickName,
                      Description = "Script variable " + NickName,
			            };
		            }
		            case GH_ParameterSide.Output:
		            {
			            return new Param_GenericObject
			            {
				              NickName = GH_ComponentParamServer.InventUniqueNickname("abcdefghijklmn", this.Params.Output),
                      Name = NickName,
                      Description = "Script variable " + NickName,
			            };
		            }
		            default:
		            {
			              return null;
		            }
            }
        }

        bool IGH_VariableParameterComponent.DestroyParameter(GH_ParameterSide side, int index)
        {
            return true;
        }

        bool IGH_VariableParameterComponent.CanInsertParameter(GH_ParameterSide side, int index)
        {
            if (side == GH_ParameterSide.Input)
                return index > (HideCodeInput ? -1 : 0);
            else if(side == GH_ParameterSide.Output)
                return index > (HideCodeOutput ? -1 : 0);
            return false;
        }

        bool IGH_VariableParameterComponent.CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return (this as IGH_VariableParameterComponent).CanInsertParameter(side, index);
        }

        public override void VariableParameterMaintenance()
        {
            foreach (var i in Params.Input)
            {
                if (i is Param_ScriptVariable)
                    FixGhInput(i as Param_ScriptVariable);
            }
            foreach (var i in Params.Output)
            {
                if (i is Param_GenericObject)
                {
                    i.Name = i.NickName;
                    
                    if(string.IsNullOrEmpty(i.Description))
                        i.Description = i.NickName;
                }
            }
        }

        protected override void SetScriptTransientGlobals()
        {
          base.SetScriptTransientGlobals();

          _py.ScriptContextDoc = _document;
          _marshal = new NewComponentIOMarshal(_document, this);
          _py.SetVariable(DOCUMENT_NAME, _document);
          _py.SetIntellisenseVariable(DOCUMENT_NAME, _document);
        }

        internal const string Id = "{410755B1-224A-4C1E-A407-BF32FB45EA7E}";

        public override Guid ComponentGuid
        {
          get { return new Guid(Id); }
        }

        #endregion
    }
}