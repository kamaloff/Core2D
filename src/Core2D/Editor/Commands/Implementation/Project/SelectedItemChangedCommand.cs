﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Core2D.Editor.Input;
using Core2D.Project;

namespace Core2D.Editor.Commands
{
    /// <inheritdoc/>
    public class SelectedItemChangedCommand : Command<XSelectable>, ISelectedItemChangedCommand
    {
        /// <inheritdoc/>
        public override bool CanRun(XSelectable item)
            => ServiceProvider.GetService<ProjectEditor>().IsEditMode();

        /// <inheritdoc/>
        public override void Run(XSelectable item)
            => ServiceProvider.GetService<ProjectEditor>().OnSelectedItemChanged(item);
    }
}
