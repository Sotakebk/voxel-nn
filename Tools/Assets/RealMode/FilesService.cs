using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace RealMode
{
    public class FilesService : MonoBehaviour
    {
        private const string ProjectRootMarkerFileName = "voxel-nn.root";
        private const string ProjectDatasetDirectoryName = "Dataset";

        public event DatasetPathUpdatedEventHandler? OnDatasetPathUpdated;

        public delegate void DatasetPathUpdatedEventHandler(FilesService sender);

        private bool _shouldRaiseEvent;
        private readonly object _lock = new object();

        private string _currentDatasetPath;
        public string CurrentDatasetPath => _currentDatasetPath;

        public void OpenDatasetDirectory(string path)
        {
            try
            {
                lock (_lock)
                {
                    if (_currentDatasetPath != path)
                    {
                        _currentDatasetPath = path;
                        _shouldRaiseEvent = true;
                    }
                }
                Debug.Log($"Loading dataset at '{path}'.");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void Start()
        {
            try
            {
                var currentDirectory = new DirectoryInfo(Application.dataPath);
                while (currentDirectory.Exists)
                {
                    if (currentDirectory.EnumerateFiles()
                        .Any(f => string.Equals(f.Name, ProjectRootMarkerFileName,
                                                StringComparison.CurrentCultureIgnoreCase)))
                    {
                        var targetDirectory = currentDirectory.EnumerateDirectories()
                            .FirstOrDefault(d => string.Equals(d.Name, ProjectDatasetDirectoryName,
                                StringComparison.CurrentCultureIgnoreCase));
                        if (targetDirectory != null)
                        {
                            OpenDatasetDirectory(targetDirectory.FullName);
                            break;
                        }
                    }
                    else
                    {
                        if (currentDirectory.Parent.Exists)
                            currentDirectory = currentDirectory.Parent;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void Update()
        {
            var raisingEvent = false;
            lock (_lock)
            {
                if (_shouldRaiseEvent)
                {
                    _shouldRaiseEvent = false;
                    raisingEvent = true;
                }
            }
            if (raisingEvent)
                OnDatasetPathUpdated?.Invoke(this);
        }

        public Palette? LoadPalette()
        {
            throw new NotImplementedException();
        }

        public void SavePalette(Palette palette)
        {
            throw new NotImplementedException();
        }
    }
}