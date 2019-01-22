﻿using System;
using System.Collections.Generic;
using System.Linq;
using Markdig;
using MLS.Agent.Markdown;
using Recipes;

namespace MLS.Agent
{
    public class MarkdownProject
    {
        internal IDirectoryAccessor DirectoryAccessor { get; }

        private readonly Dictionary<RelativeFilePath, MarkdownPipeline> _markdownPipelines = new Dictionary<RelativeFilePath, MarkdownPipeline>();

        public MarkdownProject(IDirectoryAccessor directoryAccessor)
        {
            DirectoryAccessor = directoryAccessor ?? throw new ArgumentNullException(nameof(directoryAccessor));
        }

        public IEnumerable<MarkdownFile> GetAllMarkdownFiles() =>
            DirectoryAccessor.GetAllFilesRecursively()
                             .Where(file => file.Extension == ".md")
                             .Select(file => new MarkdownFile(file, this));

        public bool TryGetHtmlContent(RelativeFilePath path, out string html)
        {
            if (!DirectoryAccessor.FileExists(path))
            {
                html = null;
                return false;
            }

            html = ConvertToHtml(path, DirectoryAccessor.ReadAllText(path));

            return true;
        }

        private string ConvertToHtml(RelativeFilePath filePath, string content)
        {
            var pipeline = GetMarkdownPipelineFor(filePath);

            return Markdig.Markdown.ToHtml(content, pipeline);
        }

        internal MarkdownPipeline GetMarkdownPipelineFor(RelativeFilePath filePath)
        {
            return _markdownPipelines.GetOrAdd(filePath, key =>
            {
                var relativeAccessor = DirectoryAccessor.GetDirectoryAccessorForRelativePath(filePath.Directory);

                return new MarkdownPipelineBuilder()
                       .UseCodeLinks(relativeAccessor)
                       .Build();
            });
        }
    }
}