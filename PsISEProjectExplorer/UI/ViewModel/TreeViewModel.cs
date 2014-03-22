﻿using PsISEProjectExplorer.EnumsAndOptions;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.IseIntegration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.UI.ViewModel
{
    public class TreeViewModel : BaseViewModel
    {

        public TreeViewEntryItemObservableSet TreeViewItems
        {
            get
            {
                if (this.RootTreeViewEntryItem == null)
                {
                    return new TreeViewEntryItemObservableSet();
                }
                return this.RootTreeViewEntryItem.Children;
            }
        }


        public IseIntegrator IseIntegrator { get; set; }

        private TreeViewEntryItemModel RootTreeViewEntryItem { get; set; }

        public TreeViewModel()
        {
        }

        public void RefreshFromRoot(INode newDocumentHierarchyRoot, bool expandAllNodes)
        {
            if (newDocumentHierarchyRoot == null)
            {
                this.SetNewRootItem(null);
                FileSystemChangeNotifier.Watch(null);
                return;
            }

            if (this.RootTreeViewEntryItem == null || !this.RootTreeViewEntryItem.Node.Equals(newDocumentHierarchyRoot))
            {
                TreeViewEntryItemModel newRootItem = new TreeViewEntryItemModel(newDocumentHierarchyRoot, null);
                this.SetNewRootItem(newRootItem);
                FileSystemChangeNotifier.Watch(newRootItem.Node.Path);
            }

            this.RefreshFromIntermediateNode(newDocumentHierarchyRoot, this.RootTreeViewEntryItem, expandAllNodes);
            this.OnPropertyChanged("TreeViewItems");
        }

        public TreeViewEntryItemModel FindTreeViewEntryItemByPath(string path)
        {
            return this.FindTreeViewEntryItemByPath(this.RootTreeViewEntryItem, path);
        }

        private TreeViewEntryItemModel FindTreeViewEntryItemByPath(TreeViewEntryItemModel item, string path)
        {
            foreach (TreeViewEntryItemModel child in item.Children)
            {
                if (child.Path == path)
                {
                    return child;
                }
                var result = this.FindTreeViewEntryItemByPath(child, path);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        private void SetNewRootItem(TreeViewEntryItemModel rootItem)
        {
            this.RootTreeViewEntryItem = rootItem;
        }

        private void RefreshFromIntermediateNode(INode node, TreeViewEntryItemModel treeViewEntryItem, bool expandAllNodes)
        {
            
            // delete old items
            var itemsToDelete = treeViewEntryItem.Children.Where(item => !node.Children.Contains(item.Node)).ToList();
            foreach (TreeViewEntryItemModel item in itemsToDelete) {
                item.Delete();
            }

            // add new items
            IList<INode> childrenToIterate = new List<INode>(node.Children);
            foreach (INode docHierarchyChild in childrenToIterate)
            {
                TreeViewEntryItemModel newTreeViewItem = null;
                foreach (TreeViewEntryItemModel treeViewChild in treeViewEntryItem.Children)
                {
                    if (treeViewChild.Node.Equals(docHierarchyChild))
                    {
                        newTreeViewItem = treeViewChild;
                        break;
                    }
                }
                if (newTreeViewItem == null)
                {
                    newTreeViewItem = new TreeViewEntryItemModel(docHierarchyChild, treeViewEntryItem);
                }
                else
                {
                    newTreeViewItem.UpdateNode(docHierarchyChild);
                }
                if (expandAllNodes)
                {
                    newTreeViewItem.IsExpanded = true;
                }
                this.RefreshFromIntermediateNode(docHierarchyChild, newTreeViewItem, expandAllNodes);
            }

        }

        public void SelectItem(TreeViewEntryItemModel item)
        {
            if (this.IseIntegrator == null)
            {
                throw new InvalidOperationException("IseIntegrator has not ben set yet.");
            }
            if (item != null)
            {
                if (item.Node.NodeType == NodeType.FILE)
                {
                    this.IseIntegrator.GoToFile(item.Node.Path);
                }
                else if (item.Node.NodeType == NodeType.FUNCTION)
                {
                    PowershellFunctionNode node = ((PowershellFunctionNode)item.Node);
                    this.IseIntegrator.GoToFile(node.FilePath);
                    this.IseIntegrator.SetCursor(node.PowershellFunction.StartLine, node.PowershellFunction.StartColumn);
                }
            }
        }

    }
}
