// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

using System;
using System.Collections.Generic;

namespace Azure.ResourceManager.DataBox.Models
{
    /// <summary> Request to validate subscription permission to create jobs. </summary>
    public partial class SubscriptionIsAllowedToCreateJobValidationContent : DataBoxValidationInputContent
    {
        /// <summary> Initializes a new instance of <see cref="SubscriptionIsAllowedToCreateJobValidationContent"/>. </summary>
        public SubscriptionIsAllowedToCreateJobValidationContent()
        {
            ValidationType = DataBoxValidationInputDiscriminator.ValidateSubscriptionIsAllowedToCreateJob;
        }

        /// <summary> Initializes a new instance of <see cref="SubscriptionIsAllowedToCreateJobValidationContent"/>. </summary>
        /// <param name="validationType"> Identifies the type of validation request. </param>
        /// <param name="serializedAdditionalRawData"> Keeps track of any properties unknown to the library. </param>
        internal SubscriptionIsAllowedToCreateJobValidationContent(DataBoxValidationInputDiscriminator validationType, IDictionary<string, BinaryData> serializedAdditionalRawData) : base(validationType, serializedAdditionalRawData)
        {
            ValidationType = validationType;
        }
    }
}
