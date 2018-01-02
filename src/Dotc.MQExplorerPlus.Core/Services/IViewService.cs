#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Collections.ObjectModel;
using Dotc.MQExplorerPlus.Core.ViewModels;
using Dotc.MQExplorerPlus.Core.Views;

namespace Dotc.MQExplorerPlus.Core.Services
{


    public class CountEventArgs : EventArgs
    {
        public CountEventArgs(int count)
        {
            Count = count;
        }

        public int Count { get; private set; }
    }
    public interface IViewService
    {

        ObservableCollection<IDocumentView> DocumentViews { get; }

        event EventHandler<CountEventArgs> DocumentsCountChanged;

        IDocumentView ActiveDocumentView { get; }
        DocumentViewModel FindDocumentViewById(string id);
        bool DocumentViewExists(string id);
        void SetActiveDocumentView(IDocumentView view);
        void SetActiveDocumentView(string id);
        void AddDocumentView(DocumentViewModel vm, bool setActive);



        bool IsModalOpened { get; }
        event EventHandler ModalOpening;
        event EventHandler ModalClosing;

        void ShowModalView(ModalViewModel vm, Action onClosedCallBack = null);

        IModalView CurrentModalView { get; }

        ModalViewModel CurrentModalViewModel { get; }

        void CancelModal();
    }
}
