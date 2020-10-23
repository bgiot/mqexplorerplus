#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Dotc.MQExplorerPlus.Core.Services;
using Dotc.MQExplorerPlus.Core.ViewModels;
using Dotc.MQExplorerPlus.Core.Views;
using Dotc.Mvvm;
using Dotc.Wpf;

namespace Dotc.MQExplorerPlus
{

    public class ViewService : BindableBase, IViewService
    {

        private IDocumentView _activeView;
        private readonly Dictionary<string, DocumentViewModel> _viewModels;

        public ViewService()
        {
            DocumentViews = new ObservableCollection<IDocumentView>();
            _viewModels = new Dictionary<string, DocumentViewModel>();
        }


        public ObservableCollection<IDocumentView> DocumentViews { get; }

        public event EventHandler<CountEventArgs> DocumentsCountChanged;

        public IDocumentView ActiveDocumentView
        {
            get { return _activeView; }
            set
            {
                if (_activeView != null)
                {
                    var tab = _activeView?.DataContext as DocumentViewModel;
                    tab?.OnDeactivate();
                }
                if (SetPropertyAndNotify(ref _activeView, value))
                {
                    var tab = _activeView?.DataContext as DocumentViewModel;
                    tab?.OnActivate();
                }
            }
        }

        public DocumentViewModel FindDocumentViewById(string id)
        {
            return _viewModels.ContainsKey(id) ? _viewModels[id] : null;
        }


        public bool DocumentViewExists(string id)
        {
            return _viewModels.ContainsKey(id);
        }

        public void SetActiveDocumentView(IDocumentView view)
        {
            UIDispatcher.Execute(() =>
            {
                if (DocumentViews.Contains(view))
                {
                    ActiveDocumentView = view;
                }
            });
        }

        public void SetActiveDocumentView(string id)
        {
            UIDispatcher.Execute(() =>
            {
                if (_viewModels.ContainsKey(id))
                {
                    ActiveDocumentView = (IDocumentView)_viewModels[id].View;
                }
            });
        }

        public void AddDocumentView(DocumentViewModel vm, bool setActive)
        {
            if (vm == null) throw new ArgumentNullException(nameof(vm));

            UIDispatcher.Execute(() =>
            {
                WeakEventManager<DocumentViewModel, EventArgs>
                    .AddHandler(vm, "Closed", TabVm_Closed);
                _viewModels.Add(vm.UniqueId, vm);
                DocumentViews.Add(vm.View as IDocumentView);
                if (setActive) SetActiveDocumentView(vm.View as IDocumentView);

                OnCountChanged();

            });

        }

        private void TabVm_Closed(object sender, EventArgs e)
        {
            var vm = sender as DocumentViewModel;
            if (vm != null && DocumentViews.Contains(vm.View as IDocumentView))
            {
                _viewModels.Remove(vm.UniqueId);
                DocumentViews.Remove(vm.View as IDocumentView);
                OnCountChanged();
            }
        }

        private void OnCountChanged()
        {
            if (DocumentsCountChanged != null)
            {
                DocumentsCountChanged.Invoke(this, new CountEventArgs(DocumentViews.Count));
            }
        }




        private bool _modalopened;
        private IModalView _modalview;
        private ModalViewModel _modalvm;
        private Action _onModalClosedCallBack;

        public bool IsModalOpened
        {
            get { return _modalopened; }
            set { SetPropertyAndNotify(ref _modalopened, value); }
        }

        public event EventHandler ModalOpening;
        public event EventHandler ModalClosing;

        public void ShowModalView(ModalViewModel vm, Action onClosedCallBack = null)
        {
            if (vm == null) throw new ArgumentNullException(nameof(vm));

            if (!IsModalOpened)
            {
                CurrentModalViewModel = vm;
                CurrentModalView = vm.View as IModalView;
                SetOpened();
                _onModalClosedCallBack = onClosedCallBack;
                WeakEventManager<ModalViewModel, EventArgs>
                    .AddHandler(CurrentModalViewModel, "Closed", ModalVm_Closed);

                CurrentModalViewModel.OnOpened();
            }
        }

        private void ModalVm_Closed(object sender, EventArgs args)
        {
            SetClosed();
            CurrentModalView = null;
            CurrentModalViewModel = null;
            _onModalClosedCallBack?.Invoke();
        }

        private void SetOpened()
        {
            IsModalOpened = true;
            ModalOpening?.Invoke(this, EventArgs.Empty);
        }

        private void SetClosed()
        {
            IsModalOpened = false;
            ModalClosing?.Invoke(this, EventArgs.Empty);
        }

        public IModalView CurrentModalView
        {
            get { return _modalview; }
            private set { SetPropertyAndNotify(ref _modalview, value); }
        }

        public ModalViewModel CurrentModalViewModel
        {
            get { return _modalvm; }
            private set { SetPropertyAndNotify(ref _modalvm, value); }
        }

        public void CancelModal()
        {
            CurrentModalViewModel?.DoCancel();
        }
    }
}
