# Azure Storage Data Movement Common client library for .NET

## Project Status: Beta

This product is in beta. Some features will be missing or have significant bugs. Please see [Known Issues](https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/storage/Azure.Storage.DataMovement/KnownIssues.md) for detailed information.

---

Azure Storage is a Microsoft-managed service providing cloud storage that is
highly available, secure, durable, scalable, and redundant.

The Azure Storage Data Movement library is optimized for uploading, downloading and
copying customer data.

Currently this version of the Data Movement library only supports Blobs and File Shares.

[Source code][source] | [API reference documentation][docs] | [REST API documentation][rest_docs] | [Product documentation][product_docs]

## Getting started

### Install the package

Install the Azure Storage client library for .NET you'd like to use with
[NuGet][nuget] and the `Azure.Storage.DataMovement` client library will be included:

```dotnetcli
dotnet add package Azure.Storage.DataMovement --prerelease
```

### Prerequisites

You need an [Azure subscription][azure_sub] and a
[Storage Account][storage_account_docs] to use this package.

To create a new Storage Account, you can use the [Azure Portal][storage_account_create_portal],
[Azure PowerShell][storage_account_create_ps], or the [Azure CLI][storage_account_create_cli].
Here's an example using the Azure CLI:

```Powershell
az storage account create --name MyStorageAccount --resource-group MyResourceGroup --location westus --sku Standard_LRS
```

### Authenticate the client

Authentication is specific to the targeted storage service. Please see documentation for the individual services

### Permissions

Data Movement must have appropriate permissions to the storage resources.
Permissions are specific to the type of storage Data Movement is connected to.

- [Blob storage permissions](https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/storage/Azure.Storage.DataMovement.Blobs/README.md#permissions)
- [File share permissions](https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/storage/Azure.Storage.DataMovement.Files.Shares//README.md#permissions)

## Key concepts

The Azure Storage DataMovement client library contains shared infrastructure like
[TokenCredential](https://learn.microsoft.com/dotnet/api/azure.core.tokencredential?view=azure-dotnet), [TransferManager](#setup-the-transfermanager) and [RequestFailedException][RequestFailedException].

### Thread safety
We guarantee that all client instance methods are thread-safe and independent of each other ([guideline](https://azure.github.io/azure-sdk/dotnet_introduction.html#dotnet-service-methods-thread-safety)). This ensures that the recommendation of reusing client instances is always safe, even across threads.

### Additional concepts
<!-- CLIENT COMMON BAR -->
[Client options](https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/core/Azure.Core/README.md#configuring-service-clients-using-clientoptions) |
[Accessing the response](https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/core/Azure.Core/README.md#accessing-http-response-details-using-responset) |
[Long-running operations](https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/core/Azure.Core/README.md#consuming-long-running-operations-using-operationt) |
[Handling failures](https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/core/Azure.Core/README.md#reporting-errors-requestfailedexception) |
[Diagnostics](https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/core/Azure.Core/samples/Diagnostics.md) |
[Mocking](https://learn.microsoft.com/dotnet/azure/sdk/unit-testing-mocking) |
[Client lifetime](https://devblogs.microsoft.com/azure-sdk/lifetime-management-and-thread-safety-guarantees-of-azure-sdk-net-clients/)
<!-- CLIENT COMMON BAR -->

## Examples

This section demonstrates usage of Data Movement regardless of extension package. Package-specific information and usage samples can be found in that package's documentation. These examples will use local disk and Azure Blob Storage when specific resources are needed for demonstration purposes, but the topics here will apply to other packages.

### Setup the `TransferManager`

Singleton usage of `TransferManager` is recommended. Providing `TransferManagerOptions` is optional.

```C# Snippet:CreateTransferManagerSimple_BasePackage
TransferManager transferManager = new TransferManager(new TransferManagerOptions());
```

### Starting New Transfers

Transfers are defined by a source and destination `StorageResource`. There are two kinds of `StorageResource`: `StorageResourceSingle` and `StorageResourceContainer`. Source and destination of a given transfer must be of the same kind.

`StorageResource` instances are obtained from `StorageResourceProvider` instances. See [Initializing Local File `StorageResource`](#initializing-local-file-storageresource) for more information on the resource provider for local files and directories. See the documentation for other DataMovement extension packages for more info on their `StorageResourceProvider` types.

The below sample demonstrates `StorageResourceProvider` use to start transfers by uploading a file to Azure Blob Storage, using the Azure.Storage.DataMovement.Blobs package. It uses an Azure.Core token credential with permission to write to the blob.

```C# Snippet:SimpleBlobUpload_BasePackage
LocalFilesStorageResourceProvider files = new();
BlobsStorageResourceProvider blobs = new(tokenCredential);

// Create simple transfer single blob upload job
TransferOperation transferOperation = await transferManager.StartTransferAsync(
    sourceResource: files.FromFile(sourceLocalPath),
    destinationResource: blobs.FromBlob(destinationBlobUri));
await transferOperation.WaitForCompletionAsync();
```

### Resuming Existing Transfers

By persisting transfer progress to disk, DataMovement allows resuming of transfers that failed partway through, or were otherwise paused. To resume a transfer, the transfer manager needs to be setup in the first place with `StorageResourceProvider` instances (the same ones used above in [Starting New Transfers](#starting-new-transfers)) which are capable of reassembling the transfer components from persisted data.

To pause see [Pausing Transfers](#pausing-transfers).

The below sample initializes the `TransferManager` such that it's capable of resuming transfers between the local filesystem and Azure Blob Storage, using the Azure.Storage.DataMovement.Blobs package.

**Important:** Credentials to storage providers are not persisted. Storage access which requires credentials will need its appropriate `StorageResourceProvider` to be configured with those credentials. Below uses an `Azure.Core` token credential with permission to the appropriate resources.

```C# Snippet:SetupTransferManagerForResume
LocalFilesStorageResourceProvider files = new();
BlobsStorageResourceProvider blobs = new(tokenCredential);
TransferManager transferManager = new(new TransferManagerOptions()
{
    ResumeProviders = new List<StorageResourceProvider>() { files, blobs },
});
```

To resume a transfer, provide the transfer's ID, as shown below. In the case where your application does not have the desired transfer ID available, use `TransferManager.GetTransfersAsync()` to find that transfer and it's ID.

```C# Snippet:DataMovement_ResumeSingle
TransferOperation resumedTransfer = await transferManager.ResumeTransferAsync(transferId);
```

#### Pause and Resume Checkpointing

The location of persisted transfer data will be different than the default location if `TransferCheckpointStoreOptions` were set in `TransferManagerOptions`. To resume transfers recorded in a non-default location, the transfer manager resuming the transfer will also need the appropriate checkpoint store options.

To specify the checkpoint folder directory:
```csharp
TransferManagerOptions options = new TransferManagerOptions()
{
    CheckpointerOptions = TransferCheckpointStoreOptions.Local(<directory path location>)
};
```

To disable checkpointing:
```csharp
TransferManagerOptions options = new TransferManagerOptions()
{
    CheckpointerOptions = TransferCheckpointStoreOptions.Disabled()
};
```

### Monitoring Transfers

Transfers can be observed through several mechanisms, depending on your needs.

#### With `DataTransfer`

Simple observation can be done through a `DataTransfer` instance representing an individual transfer. This is obtained on transfer start. You can also enumerate through all transfers on a `TransferManager`.

A function that writes the status of each transfer to console:

```C# Snippet:EnumerateTransfers
async Task CheckTransfersAsync(TransferManager transferManager)
{
    await foreach (TransferOperation transfer in transferManager.GetTransfersAsync())
    {
        using StreamWriter logStream = File.AppendText(logFile);
        logStream.WriteLine(Enum.GetName(typeof(TransferStatus), transfer.Status));
    }
}
```

`DataTransfer` contains property `TransferStatus`. You can read this to determine the state of the transfer. States include queued for transfer, in progress, paused, completed, and more.

`DataTransfer` also exposes a task for transfer completion, shown in [Starting New Transfers](#starting-new-transfers).

#### With Events via `TransferOptions`

When starting a transfer, `TransferOptions` contains multiple events that can be listened to for observation. Below demonstrates listening to the event for individual file completion and logging the result.

A function that listens to status events for a given transfer:

```C# Snippet:ListenToTransferEvents
async Task<TransferOperation> ListenToTransfersAsync(TransferManager transferManager,
    StorageResource source, StorageResource destination)
{
    TransferOptions transferOptions = new();
    transferOptions.ItemTransferCompleted += (TransferItemCompletedEventArgs args) =>
    {
        using StreamWriter logStream = File.AppendText(logFile);
        logStream.WriteLine($"File Completed Transfer: {args.Source.Uri.LocalPath}");
        return Task.CompletedTask;
    };
    return await transferManager.StartTransferAsync(
        source,
        destination,
        transferOptions);
}
```

#### With IProgress via `TransferOptions`

When starting a transfer, `TransferOptions` allows setting a progress handler that contains the progress information for the overall transfer. Granular progress updates will be communicated to the provided `IProgress` instance.

A function that listens to progress updates for a given transfer with a supplied `IProgress<TStorageTransferProgress>`:

```C# Snippet:ListenToProgress
async Task<TransferOperation> ListenToProgressAsync(TransferManager transferManager, IProgress<TransferProgress> progress,
    StorageResource source, StorageResource destination)
{
    TransferOptions transferOptions = new()
    {
        ProgressHandlerOptions = new()
        {
            ProgressHandler = progress,
            // optionally include the below if progress updates on bytes transferred are desired
            TrackBytesTransferred = true,
        }
    };
    return await transferManager.StartTransferAsync(
        source,
        destination,
        transferOptions);
}
```

### Pausing transfers

Transfers can be paused either by a given `DataTransfer` or through the `TransferManager` handling the transfer by referencing the transfer ID. The ID can be found on the `DataTransfer` object you received upon transfer start.

```C# Snippet:PauseFromTransfer
await transferOperation.PauseAsync();
```

```C# Snippet:PauseFromManager
await transferManager.PauseTransferAsync(transferId);
```

### Handling Failed Transfers

Transfer failure can be observed by checking the `DataTransfer` status upon completion, or by listening to failure events on the transfer. While checking the `DataTransfer` may be sufficient for handling single-file transfer failures, event listening is recommended for container transfers.

Below logs failure for a single transfer by checking its status after completion.

```C# Snippet:LogTotalTransferFailure
await dataTransfer2.WaitForCompletionAsync();
if (dataTransfer2.Status.State == TransferState.Completed
    && dataTransfer2.Status.HasFailedItems)
{
    using (StreamWriter logStream = File.AppendText(logFile))
    {
        logStream.WriteLine($"Failure for TransferId: {dataTransfer2.Id}");
    }
}
```

Below logs individual failures in a container transfer via `TransferOptions` events.

```C# Snippet:LogIndividualTransferFailures
transferOptions.ItemTransferFailed += (TransferItemFailedEventArgs args) =>
{
    using (StreamWriter logStream = File.AppendText(logFile))
    {
        // Specifying specific resources that failed, since its a directory transfer
        // maybe only one file failed out of many
        logStream.WriteLine($"Exception occurred with TransferId: {args.TransferId}," +
            $"Source Resource: {args.Source.Uri.AbsoluteUri}, +" +
            $"Destination Resource: {args.Destination.Uri.AbsoluteUri}," +
            $"Exception Message: {args.Exception.Message}");
    }
    return Task.CompletedTask;
};
```

### Initializing Local File or Directory `StorageResource`

Local filesystem resources are provided by `LocalFilesStorageResourceProvider`. This provider requires no setup to produce storage resources.

```csharp
LocalFilesStorageResourceProvider files = new();
StorageResource fileResource = files.FromFile("C:/path/to/file.txt");
StorageResource directoryResource = files.FromDirectory("C:/path/to/dir");
```

## Troubleshooting

See [Handling Failed Transfers](#handling-failed-transfers) and [Enabling Logging](https://learn.microsoft.com/dotnet/azure/sdk/logging) to assist with any troubleshooting.

## Next steps

Get started with our [Blob DataMovement samples][blob_samples].

Get started with our [Share File DataMovement samples][share_samples].

## Contributing

See the [Storage CONTRIBUTING.md][storage_contrib] for details on building,
testing, and contributing to these libraries.

This project welcomes contributions and suggestions.  Most contributions require
you to agree to a Contributor License Agreement (CLA) declaring that you have
the right to, and actually do, grant us the rights to use your contribution. For
details, visit [cla.microsoft.com][cla].

This project has adopted the [Microsoft Open Source Code of Conduct][coc].
For more information see the [Code of Conduct FAQ][coc_faq]
or contact [opencode@microsoft.com][coc_contact] with any
additional questions or comments.

![Impressions](https://azure-sdk-impressions.azurewebsites.net/api/impressions/azure-sdk-for-net%2Fsdk%2Fstorage%2FAzure.Storage.Common%2FREADME.png)

<!-- LINKS -->
[source]: https://github.com/Azure/azure-sdk-for-net/tree/main/sdk/storage/Azure.Storage.DataMovement/src
[docs]: https://learn.microsoft.com/dotnet/api/azure.storage
[rest_docs]: https://learn.microsoft.com/rest/api/storageservices/
[product_docs]: https://learn.microsoft.com/azure/storage/
[nuget]: https://www.nuget.org/
[storage_account_docs]: https://learn.microsoft.com/azure/storage/common/storage-account-overview
[storage_account_create_ps]: https://learn.microsoft.com/azure/storage/common/storage-quickstart-create-account?tabs=azure-powershell
[storage_account_create_cli]: https://learn.microsoft.com/azure/storage/common/storage-quickstart-create-account?tabs=azure-cli
[storage_account_create_portal]: https://learn.microsoft.com/azure/storage/common/storage-quickstart-create-account?tabs=azure-portal
[azure_cli]: https://learn.microsoft.com/cli/azure
[azure_sub]: https://azure.microsoft.com/free/dotnet/
[auth_credentials]: https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/storage/Azure.Storage.Common/src/StorageSharedKeyCredential.cs
[blobs_examples]: https://github.com/Azure/azure-sdk-for-net/tree/main/sdk/storage/Azure.Storage.DataMovement.Blobs#examples
[RequestFailedException]: https://github.com/Azure/azure-sdk-for-net/tree/main/sdk/core/Azure.Core/src/RequestFailedException.cs
[error_codes]: https://learn.microsoft.com/rest/api/storageservices/common-rest-api-error-codes
[blob_samples]: https://github.com/Azure/azure-sdk-for-net/tree/main/sdk/storage/Azure.Storage.DataMovement.Blobs/samples
[share_samples]: https://github.com/Azure/azure-sdk-for-net/tree/main/sdk/storage/Azure.Storage.DataMovement.Files.Shares/samples
[storage_contrib]: https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/storage/CONTRIBUTING.md
[cla]: https://cla.microsoft.com
[coc]: https://opensource.microsoft.com/codeofconduct/
[coc_faq]: https://opensource.microsoft.com/codeofconduct/faq/
[coc_contact]: mailto:opencode@microsoft.com
