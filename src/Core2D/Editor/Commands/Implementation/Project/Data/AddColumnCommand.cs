﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Core2D.Data.Database;
using Core2D.Editor.Input;

namespace Core2D.Editor.Commands
{
    /// <inheritdoc/>
    public class AddColumnCommand : Command<XDatabase>, IAddColumnCommand
    {
        /// <inheritdoc/>
        public override bool CanRun(XDatabase db)
            => ServiceProvider.GetService<ProjectEditor>().IsEditMode();

        /// <inheritdoc/>
        public override void Run(XDatabase db)
            => ServiceProvider.GetService<ProjectEditor>().OnAddColumn(db);
    }
}
