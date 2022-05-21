using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrooxEngine;
using BaseX;

namespace ShapekeyDynvarGenerator
{
    public class ShapekeyDynvarGenerator : ToolTip
    {
        // When the component is attached to a slot
        protected override void OnAttach()
        {
            // Default tooltip attach logic
            base.OnAttach();

            // Add a child slot which will hold the visuals
            var visual = Slot.AddSlot("Visual");

            // Let's attach a collider, which will ensure it can be grabbed and equipped
            var coneCollider = visual.AttachComponent<ConeCollider>();
            coneCollider.Radius.Value = 0.015f;
            coneCollider.Height.Value = 0.05f;

            // Rotate the visual (cones are pointing upwards)
            visual.LocalRotation = floatQ.Euler(90, 0, 0);
            // Offset it a little forwards
            visual.LocalPosition += float3.Forward * 0.05f;

            // Let's create a new material for the tooltip model
            var material = visual.AttachComponent<PBS_Metallic>();
            material.AlbedoColor.Value = new color(1f, 0.75f, 0f); // make the material golden to distinguish the tooltip a bit

            // Attach a cone mesh. The AttachMesh<MeshType> is a shorthand for attaching MeshRenderer
            // a mesh provider and setting up the references appropriatelly
            var cone = visual.AttachMesh<ConeMesh>(material);

            // Set the mesh parameters
            cone.RadiusBase.Value = 0.015f;
            cone.Height.Value = 0.05f;
        }

        // When the primary button of the controller has been pressed
        public override void OnPrimaryPress()
        {
            // Regular tooltip press things
            base.OnPrimaryPress();

            // Gets a raycast hit from the tooltop
            RaycastHit? nullableHit = this.GetHit();

            if (nullableHit != null)
            {
                RaycastHit hit = nullableHit ?? new RaycastHit();

                Slot hitSlot = hit.Collider.Slot;

                ContainerWorker<Component>.ComponentEnumerable components = hitSlot.Components;

                hitSlot.ForeachComponentInChildren<Component>(ProcessComponent, false, false);
            }
        }

        public bool ProcessComponent(Component component)
        {
            // If it finds a mesh renderer
            if (component.GetType().ToString() == "FrooxEngine.SkinnedMeshRenderer")
            {
                // Cast to a mesh renderer
                SkinnedMeshRenderer meshRenderer = (SkinnedMeshRenderer)component;

                // If it has blendshapes
                if (meshRenderer.BlendShapeCount > 0)
                {
                    // Add the child slot to put the components on
                    Slot shapeKeySlot = component.Slot.AddSlot("ShapeKeys", true);

                    // Create a dynamic field for each blendshape
                    for (int i = 0; i < meshRenderer.BlendShapeCount; i++)
                    {
                        DynamicField<float> dynamicField = shapeKeySlot.AttachComponent<DynamicField<float>>();

                        dynamicField.VariableName.ForceSet(meshRenderer.BlendShapeName(i));

                        dynamicField.TargetField.Target = meshRenderer.GetBlendShape(meshRenderer.BlendShapeName(i));
                    }
                }
            }

            return true;
        }
    }
}
