// System
global using System.IO;
// Microsoft
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Logging.Testing;
// Third-party
global using NSubstitute;
global using NSubstitute.ExceptionExtensions;
global using NUnit.Framework;
// Projects
global using PhotoManager.Common;
global using PhotoManager.Domain;
global using PhotoManager.Domain.Interfaces;
global using PhotoManager.Domain.UserConfigurationSettings;
global using PhotoManager.Infrastructure;
global using PhotoManager.Infrastructure.Database;
global using PhotoManager.Infrastructure.Database.Storage;
global using PhotoManager.Infrastructure.TablesConfig;
global using PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;
global using PhotoManager.UI.Converters;
global using PhotoManager.UI.ViewModels;
// Aliases
global using Rotation = PhotoManager.Common.ImageRotation;
