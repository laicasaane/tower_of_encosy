using System;
using System.Diagnostics.CodeAnalysis;
using EncosyTower.Collections;
using EncosyTower.StringIds;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace EncosyTower.VisualDebugging.Commands
{
    public class VisualCommanderView : VisualElement, IDisposable
    {
        public static readonly string UssClassName = "visual-commander";
        public static readonly string DirectoryScrollUssClassName = $"{UssClassName}__directory--srcoll";
        public static readonly string DirectoryContainerUssClassName = $"{UssClassName}__directory--container";
        public static readonly string CommandScrollUssClassName = $"{UssClassName}__command--scroll";
        public static readonly string CommandContainerUssClassName = $"{UssClassName}__command--container";

        private readonly VisualElement _directoryContainer;
        private readonly VisualElement _commandContainer;

        // Detect redundant Dispose() calls.
        private bool _isDisposed;

        public VisualCommanderView(float directoryListWidth)
        {
            AddToClassList(UssClassName);

            var splitter = new TwoPaneSplitView(0, directoryListWidth, TwoPaneSplitViewOrientation.Horizontal);
            Add(splitter);

            var directoryScroll = CreateContainer(
                  splitter
                , DirectoryScrollUssClassName
            );

            directoryScroll = directoryScroll.Q("unity-content-container");

            var radioGroup = new RadioButtonGroup(string.Empty);
            radioGroup.AddToClassList(DirectoryContainerUssClassName);
            directoryScroll.Add(radioGroup);

            _directoryContainer = radioGroup;

            var commandContainer = CreateContainer(
                  splitter
                , CommandScrollUssClassName
            );

            _commandContainer = commandContainer.Q("unity-content-container");
            _commandContainer.AddToClassList(CommandContainerUssClassName);
        }

        public VisualElement DirectoryContainer => _directoryContainer;

        public VisualElement CommandContainer => _commandContainer;

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            if (disposing == false)
            {
                return;
            }

            // Dispose managed state.

            if (userData is VisualCommanderViewController controller)
            {
                controller.Dispose();
            }
        }

        private static VisualElement CreateContainer(VisualElement root, string ussClassName)
        {
            var scroll = new ScrollView();
            scroll.AddToClassList(ussClassName);
            root.Add(scroll);

            return scroll;
        }
    }

    public class VisualCommanderViewController : IDisposable
    {
        private readonly VisualCommanderView _view;
        private readonly ObjectPool<VisualDirectoryView> _directoryViewPool;
        private readonly ObjectPool<VisualCommandView> _commandViewPool;
        private readonly ObjectPool<VisualPropertyView> _propertyViewPool;
        private readonly FasterList<VisualDirectoryView> _directoryViews;
        private readonly FasterList<VisualCommandView> _commandViews;

        private ReadOnlyArrayMap<StringId, FasterList<VisualCommandData>> _directoryToCommands;
        private FasterList<VisualDirectoryData>.ReadOnly _directories;

        // Detect redundant Dispose() calls.
        private bool _isDisposed;

        public VisualCommanderViewController([NotNull] VisualCommanderView view)
        {
            _view = view;
            _view.userData = this;

            _directoryViewPool = new ObjectPool<VisualDirectoryView>(
                  VisualDirectoryAPI.CreateView
                , actionOnRelease: VisualDirectoryAPI.ReleaseView
                , defaultCapacity: 0
            );

            _commandViewPool = new(
                  VisualCommandAPI.CreateView
                , actionOnRelease: VisualCommandAPI.ReleaseView
                , defaultCapacity: 0
            );

            _propertyViewPool = new(
                  VisualPropertyAPI.CreateView
                , actionOnRelease: VisualPropertyAPI.ReleaseView
                , defaultCapacity: 0
            );

            _directoryViews = new FasterList<VisualDirectoryView>();
            _commandViews = new FasterList<VisualCommandView>();
        }

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Initialize(
              [NotNull] ReadOnlyArrayMap<StringId, FasterList<VisualCommandData>> directoryToCommands
            , [NotNull] FasterList<VisualDirectoryData>.ReadOnly directories
        )
        {
            _directoryToCommands = directoryToCommands;
            _directories = directories;

            ClearDirectories();
            ClearCommands();

            if (GenerateDirectories(out var firstData, out var firstView))
            {
                firstView.value = true;
                OnDirectorySelected(firstData);
            }
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            if (disposing == false)
            {
                return;
            }

            // Dispose managed state.
            _directoryViewPool.Dispose();
            _commandViewPool.Dispose();
            _propertyViewPool.Dispose();
        }

        private bool GenerateDirectories(
              out VisualDirectoryData firstData
            , out VisualDirectoryView firstView
        )
        {
            var directoryContainer = _view.DirectoryContainer;
            var directories = _directories.AsSpan();
            var directoryViewPool = _directoryViewPool;
            var directoryViews = _directoryViews;

            directoryViews.IncreaseCapacityTo(directories.Length);

            firstData = null;
            firstView = null;

            foreach (var data in directories)
            {
                var view = directoryViewPool.Get();

                if (view.userData is not VisualDirectoryViewController controller)
                {
                    controller = new VisualDirectoryViewController(view);
                }

                controller.Initialize(data, OnDirectorySelected);
                directoryViews.Add(view);
                directoryContainer.Add(view);

                firstData ??= data;
                firstView ??= view;
            }

            return firstData is not null;
        }

        private void ClearDirectories()
        {
            var container = _view.DirectoryContainer;
            container.Clear();

            var views = _directoryViews;
            var viewPool = _directoryViewPool;

            foreach (var view in views)
            {
                viewPool.Release(view);
            }

            views.Clear();
        }

        private void ClearCommands()
        {
            var container = _view.CommandContainer;
            container.Clear();

            var views = _commandViews;
            var viewPool = _commandViewPool;

            foreach (var view in views)
            {
                viewPool.Release(view);
            }

            views.Clear();
        }

        private void OnDirectorySelected(VisualDirectoryData data)
        {
            ClearCommands();

            if (data is null)
            {
                return;
            }

            var commandMap = _directoryToCommands;

            if (commandMap.TryGetValue(data.Id, out var commands) == false)
            {
                return;
            }

            var commandViewPool = _commandViewPool;
            var propertyViewPool = _propertyViewPool;
            var commandViews = _commandViews;
            var container = _view.CommandContainer;

            foreach (var command in commands)
            {
                var view = commandViewPool.Get();

                if (view.userData is not VisualCommandViewController controller)
                {
                    controller = new VisualCommandViewController(view, propertyViewPool);
                }

                controller.Initialize(command);
                commandViews.Add(view);
                container.Add(view);
            }
        }
    }
}
