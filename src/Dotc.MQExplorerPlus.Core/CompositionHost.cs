#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using Dotc.Wpf;

namespace Dotc.MQExplorerPlus.Core
{
    public static class CompositionHost
    {
        private static CompositionContainer _container;
        private static bool _initialized;

        public static void Initialize(CompositionContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            _container = container;
            _initialized = true;

        }

        private static void EnsureInitialized()
        {
            if (!_initialized)
                throw new MQExplorerPlusException("CompositionHost not initialized!");
        }

        public static T GetInstance<T>()
        {
            EnsureInitialized();
            return UIDispatcher.Execute<T>(() => _container.GetExportedValue<T>());
        }

        public static Lazy<T> GetLazyInstance<T>()
        {
            EnsureInitialized();
            return UIDispatcher.Execute<Lazy<T>>(() => _container.GetExport<T>());
        }

        public static IEnumerable<T> GetInstances<T>()
        {
            EnsureInitialized();
            return UIDispatcher.Execute<IEnumerable<T>>(() => _container.GetExportedValues<T>());
        }

        public static void Release()
        {
            _container?.Dispose();
        }
    }
}
