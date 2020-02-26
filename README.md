# Pericia.Storage

[![Build status](https://dev.azure.com/glacasa/GithubBuilds/_apis/build/status/Pericia.Storage-CI)](https://dev.azure.com/glacasa/GithubBuilds/_build/latest?definitionId=64)

Storage abstraction to store file in local file system or cloud storage. 

Providers available for :

- File system
- OpenStack
- Azure Blobs
- AWS S3
- In-memory

## How to use

The Pericia.Storage library provides an abstraction for cloud file storages. It is basically two interfaces :

- `IFileStorage` : represents a file storage services, which contains containers (identified by name)
- `IFileStorageContainer` : represents a container in the service, where the files will be stored.

`IFileStorage` contains the following method :

- `IFileStorageContainer GetContainer(string container);` : get a container for the current storage service.

`IFileStorageContainer` contains the following methods :

- `Task<string> SaveFile(Stream fileData)` : save a file to the storage service, and returns the unique id of the file. 

- `Task<string> SaveFile(Stream fileData, string fileId)` : Save a file to the storage service, using its unique identifier. If a file with the same id exists, it is overwritten.

- `Task<Stream> GetFile(string fileId)` : get the file content from the service

- `Task DeleteFile(string fileId)` : delete the file from the service

## Usage

You can use the service using one of the providers (see below for details on each provider). First thing to do is to add the Nuget package for your provider.

### Direct usage

Each provider has a constructor with a single parameter containing the options (specific for each provider). 

	FileSystemStorageOptions options = new FileSystemStorageOptions() { Path = @"C:\files\" };
	var fileService = new FileSystemStorage(options);

	var container1 = fileService.GetContainer("container1");

Or you can directly use the container class :

	var container2 = new FileSystemStorageContainer(options, "container2");

### Usage with IoC

The packages contains helpers to register the services in you aspnet core IoC.
First, use AddStorage to register the storage service, then add your provider, with its configuration :

	public void ConfigureServices(IServiceCollection services)
	{
		var storageConfig = Configuration.GetSection("Storage");
		services.AddStorage().AddFileSystem(storageConfig);
	}

If you want to be able to switch provider in your configuration, you can set the provider name in the `Provider` property of the options. Then you can add several providers, only the one in configuration will be registered :

	services.AddStorage().AddFileSystem(storageConfig).AddAzure(storageConfig).AddOpenStack(storageConfig);

If you only want to use one container in your app, you can register it directly :

	services.AddStorage().AddFileSystem(storageConfig).AddContainer("container1");

## Providers

### File System

[![NuGet](https://img.shields.io/nuget/v/Pericia.Storage.FileSystem.svg)](https://www.nuget.org/packages/Pericia.Storage.FileSystem/)

This provider saves the files on the hard drive.

The only option needed is `Path`, the directory path.

### OpenStack

[![NuGet](https://img.shields.io/nuget/v/Pericia.Storage.OpenStack.svg)](https://www.nuget.org/packages/Pericia.Storage.OpenStack/)

Saves to OpenStack. The following options are needed :

- `ApiEndpoint` : the openstack api url
- `AuthEndpoint` : the authentication url
- `TenantName`
- `UserId`
- `Password`
- `AuthApiVersion` : set this value to `2` if you want to use the v2 authentication API. If not set, the V3 API will be used.

### Azure blobs

[![NuGet](https://img.shields.io/nuget/v/Pericia.Storage.Azure.svg)](https://www.nuget.org/packages/Pericia.Storage.Azure/)

Saves the files to Azure Blob storage.

The only option needed is the `ConnectionString`

### AWS S3

[![NuGet](https://img.shields.io/nuget/v/Pericia.Storage.Aws.svg)](https://www.nuget.org/packages/Pericia.Storage.Aws/)

Saves the files to AWS S3.

The options needed are the `AccessKey`, `SecretKey` and the `RegionEndpoint`

### In-memory

[![NuGet](https://img.shields.io/nuget/v/Pericia.Storage.InMemory.svg)](https://www.nuget.org/packages/Pericia.Storage.InMemory/)

Store data in-memory - for testing purpose.