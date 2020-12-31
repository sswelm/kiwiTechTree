﻿using System.Linq;

namespace KiwiTechTree
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class PluginHelper: BlankMonoBehaviour
    {
        private bool _hasRunOnce;

        public void Update()
        {
            if (_hasRunOnce)
                return;

            _hasRunOnce = true;

            AddMissingTechNodes();
        }

        private void AddMissingTechNodes()
        {
            if (ResearchAndDevelopment.Instance == null) return;

            AssetBase.RnDTechTree.ReLoad();
            var rdNodes = AssetBase.RnDTechTree.GetTreeNodes().ToList();

            if (!(rdNodes[0].FindNodeByID("start", rdNodes) is ProtoRDNode startNode)) return;

            foreach (var childNode in startNode.children)
            {
                UnlockChildrenRecursively(childNode);
            }
        }

        private void UnlockChildrenRecursively(ProtoRDNode parent)
        {
            foreach (var childNode in parent.children)
            {
                UnlockChildrenRecursively(childNode);
            }

            UnlockParent(parent);
        }

        private void UnlockParent(ProtoRDNode childNode)
        {
            if (childNode.tech.state == RDTech.State.Unavailable) return;

            if (childNode.AnyParentToUnlock && childNode.parents.Any(p => p.tech.state == RDTech.State.Available))
                return;

            foreach (ProtoRDNode parentNode in childNode.parents)
            {
                if (parentNode.tech.state == RDTech.State.Available) continue;

                Unlock(parentNode);
            }
        }

        private static void Unlock(ProtoRDNode techNode)
        {
            techNode.tech.state = RDTech.State.Available;
            ResearchAndDevelopment.Instance.SetTechState(techNode.tech.techID, techNode.tech);
            ResearchAndDevelopment.Instance.UnlockProtoTechNode(techNode.tech);
        }
    }
}