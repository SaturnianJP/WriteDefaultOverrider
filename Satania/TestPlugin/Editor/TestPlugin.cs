#region usings

using nadena.dev.ndmf;
using satania.ndmfscript.test;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;

#endregion

[assembly: ExportsPlugin(typeof(WriteDefaultOverrider))]

namespace satania.ndmfscript.test
{
    public class WriteDefaultOverrider : Plugin<WriteDefaultOverrider>
    {
        /// <summary>
        /// This name is used to identify the plugin internally, and can be used to declare BeforePlugin/AfterPlugin
        /// dependencies. If not set, the full type name will be used.
        /// </summary>
        public override string QualifiedName => "WriteDefaultOverrider";

        /// <summary>
        /// The plugin name shown in debug UIs. If not set, the qualified name will be shown.
        /// </summary>
        public override string DisplayName => "Override Writedefaults";

        private static void Override_WriteDefault(AnimatorStateMachine stateMachine, bool writedefault)
        {
            // 全てのステートを処理
            foreach (var state in stateMachine.states)
            {
                state.state.writeDefaultValues = writedefault;
            }

            // サブステートマシンを再帰的に処理
            foreach (var subStateMachine in stateMachine.stateMachines)
            {
                Override_WriteDefault(subStateMachine.stateMachine, writedefault);
            }
        }

        protected override void Configure()
        {
            var seq = InPhase(BuildPhase.Resolving);

            InPhase(BuildPhase.Transforming).Run("Override Writedefaults", ctx =>
            {
                var obj = ctx.AvatarRootObject.GetComponentInChildren<WriteDefaultOverriderBehaviour>();
                if (obj != null)
                {
                    for (int i = 0; i < ctx.AvatarDescriptor.baseAnimationLayers.Length; i++)
                    {
                        var layer = ctx.AvatarDescriptor.baseAnimationLayers[i];
                        if (layer.animatorController == null || layer.type != VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.FX)
                            continue;

                        string assetPath = AssetDatabase.GetAssetPath(layer.animatorController);
                        if (string.IsNullOrEmpty(assetPath))
                            continue;

                        var animatorcontroller = AnimatorControllerUtility.DuplicateAnimationLayerController(assetPath, "Packages/nadena.dev.ndmf/__Generated", AssetDatabase.AssetPathToGUID(assetPath));

                        ctx.AvatarDescriptor.baseAnimationLayers[i].animatorController = animatorcontroller;

                        foreach (var animatorLayer in animatorcontroller.layers)
                        {
                            Override_WriteDefault(animatorLayer.stateMachine, obj.isWriteDefault);
                        }
                    }
                }
            });
        }
    }

}