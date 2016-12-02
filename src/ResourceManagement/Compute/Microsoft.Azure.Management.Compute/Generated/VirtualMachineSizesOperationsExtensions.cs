// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
// 
// Code generated by Microsoft (R) AutoRest Code Generator 0.17.0.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace Microsoft.Azure.Management.Compute
{
    using System.Threading.Tasks;
   using Microsoft.Rest.Azure;
   using Models;

    /// <summary>
    /// Extension methods for VirtualMachineSizesOperations.
    /// </summary>
    public static partial class VirtualMachineSizesOperationsExtensions
    {
            /// <summary>
            /// Lists all available virtual machine sizes for a subscription in a location.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='location'>
            /// The location upon which virtual-machine-sizes is queried.
            /// </param>
            public static System.Collections.Generic.IEnumerable<VirtualMachineSize> List(this IVirtualMachineSizesOperations operations, string location)
            {
                return System.Threading.Tasks.Task.Factory.StartNew(s => ((IVirtualMachineSizesOperations)s).ListAsync(location), operations, System.Threading.CancellationToken.None, System.Threading.Tasks.TaskCreationOptions.None, System.Threading.Tasks.TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
            }

            /// <summary>
            /// Lists all available virtual machine sizes for a subscription in a location.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='location'>
            /// The location upon which virtual-machine-sizes is queried.
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<System.Collections.Generic.IEnumerable<VirtualMachineSize>> ListAsync(this IVirtualMachineSizesOperations operations, string location, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
            {
                using (var _result = await operations.ListWithHttpMessagesAsync(location, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

    }
}