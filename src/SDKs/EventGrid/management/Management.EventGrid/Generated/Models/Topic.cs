// <auto-generated>
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Microsoft.Azure.Management.EventGrid.Models
{
    using Microsoft.Rest;
    using Microsoft.Rest.Serialization;
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// EventGrid Topic
    /// </summary>
    [Rest.Serialization.JsonTransformation]
    public partial class Topic : TrackedResource
    {
        /// <summary>
        /// Initializes a new instance of the Topic class.
        /// </summary>
        public Topic()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the Topic class.
        /// </summary>
        /// <param name="location">Location of the resource</param>
        /// <param name="id">Fully qualified identifier of the resource</param>
        /// <param name="name">Name of the resource</param>
        /// <param name="type">Type of the resource</param>
        /// <param name="tags">Tags of the resource</param>
        /// <param name="provisioningState">Provisioning state of the topic.
        /// Possible values include: 'Creating', 'Updating', 'Deleting',
        /// 'Succeeded', 'Canceled', 'Failed'</param>
        /// <param name="endpoint">Endpoint for the topic.</param>
        public Topic(string location, string id = default(string), string name = default(string), string type = default(string), IDictionary<string, string> tags = default(IDictionary<string, string>), string provisioningState = default(string), string endpoint = default(string))
            : base(location, id, name, type, tags)
        {
            ProvisioningState = provisioningState;
            Endpoint = endpoint;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets provisioning state of the topic. Possible values include:
        /// 'Creating', 'Updating', 'Deleting', 'Succeeded', 'Canceled',
        /// 'Failed'
        /// </summary>
        [JsonProperty(PropertyName = "properties.provisioningState")]
        public string ProvisioningState { get; private set; }

        /// <summary>
        /// Gets endpoint for the topic.
        /// </summary>
        [JsonProperty(PropertyName = "properties.endpoint")]
        public string Endpoint { get; private set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public override void Validate()
        {
            base.Validate();
        }
    }
}