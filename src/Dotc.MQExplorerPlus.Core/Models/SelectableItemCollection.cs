#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using Dotc.Wpf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace Dotc.MQExplorerPlus.Core.Models
{

    internal interface ISelectedEventHandler
    {
        void SelectedChanged(SelectableItem sender);
    }

    public class SelectableItemCollection<T> : ObservableCollection<T>, ISelectedEventHandler where T : SelectableItem
    {
        public SelectableItemCollection()
        {
        }

        public event EventHandler<PropertyChangedEventArgs> CountChanged;


        public void AddRange(IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            _suppressNotification = true;
            foreach (var item in items)
            {
                Add(item);
            }
            _suppressNotification = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            AddAndUpdateCount(items);

        }

        protected override void ClearItems()
        {
            foreach (T si in base.Items)
            {
                si.Detach();
            }

            base.ClearItems();
            SelectedCount = 0;
            TotalCount = 0;
        }

        private bool _suppressNotification = false;
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_suppressNotification || e == null)
                return;

            base.OnCollectionChanged(e);

            if (e.NewItems != null)
            {
                AddAndUpdateCount(e.NewItems);
            }
            if (e.OldItems != null)
            {
                RemoveAndUpdateCount(e.OldItems);
            }
        }

        private void RemoveAndUpdateCount(IEnumerable oldItems)
        {
            var selected = 0;
            var total = 0;
            foreach (SelectableItem si in oldItems)
            {
                total++;
                if (si.Selected) selected++;
                si.Detach();
            }
            TotalCount -= total;
            SelectedCount -= selected;
        }

        private void AddAndUpdateCount(IEnumerable newItems)
        {
            var selected = 0;
            var total = 0;
            foreach (SelectableItem si in newItems)
            {
                total++;
                if (si.Selected) selected++;
                si.Attach(this);
            }
            TotalCount += total;
            SelectedCount += selected;
        }

        void ISelectedEventHandler.SelectedChanged(SelectableItem sender)
        {
            SelectedCount = sender.Selected ? SelectedCount + 1 : SelectedCount - 1;
        }

        private int _totalCount;
        public int TotalCount
        {
            get
            {
                return _totalCount;
            }
            set
            {
                if (_totalCount != value)
                {
                    _totalCount = value;
                    OnCountChanged(new PropertyChangedEventArgs("TotalCount"));
                }
            }
        }

        private int _selectedCount;
        public int SelectedCount
        {
            get
            {
                return _selectedCount;
            }
            set
            {
                if (_selectedCount != value)
                {
                    _selectedCount = value;
                    OnCountChanged(new PropertyChangedEventArgs("SelectedCount"));
                }
            }
        }

        private void OnCountChanged(PropertyChangedEventArgs arg)
        {
            OnPropertyChanged(arg);
            CountChanged?.Invoke(this, arg);
        }

        public IList<T> SelectedItems()
        {
            return Items?.Where(i => i.Selected).ToList();
        }

    }
}
