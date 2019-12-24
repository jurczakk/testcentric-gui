﻿// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric GUI contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using TestCentric.Engine.Extensibility;

namespace TestCentric.Gui.Views
{
    public interface IAddinsView : IDialog
    {
        void AddExtensionPoint(IExtensionPoint extensionPoint);
    }
}
