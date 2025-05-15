// <copyright file="ContractRenewalApiController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// API controller for managing contract renewals.
    /// </summary>
    [Authorize]
    [Route("api/contract-renewals")]
    [ApiController]
    public class ContractRenewalApiController : ControllerBase
    {
        private readonly IContractRenewalRepository contractRenewalRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractRenewalApiController"/> class.
        /// </summary>
        /// <param name="contractRenewalRepository">The contract renewal repository dependency.</param>
        public ContractRenewalApiController(IContractRenewalRepository contractRenewalRepository)
        {
            this.contractRenewalRepository = contractRenewalRepository;
        }

        /// <summary>
        /// Adds a renewed contract.
        /// </summary>
        /// <param name="contract">The contract entity to add.</param>
        /// <returns>An ActionResult indicating success or failure.</returns>
        [HttpPost("add-renewed")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> AddRenewedContract([FromBody] Contract contract)
        {
            if (contract == null)
            {
                return this.BadRequest("Valid Contract entity required.");
            }

            try
            {
                await this.contractRenewalRepository.AddRenewedContractAsync(contract);
                return this.Created();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while adding renewed contract. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all renewed contracts.
        /// </summary>
        /// <returns>An ActionResult containing a list of renewed contracts.</returns>
        [HttpGet("renewed")]
        [ProducesResponseType(typeof(List<Contract>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Contract>>> GetRenewedContracts()
        {
            try
            {
                List<IContract> iContracts = await this.contractRenewalRepository.GetRenewedContractsAsync();
                List<Contract> contracts = iContracts.Cast<Contract>().ToList();
                return this.Ok(contracts);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving renewed contracts. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if a contract has been renewed.
        /// </summary>
        /// <param name="contractId">The ID of the contract to check.</param>
        /// <returns>An ActionResult containing true if the contract has been renewed, false otherwise.</returns>
        [HttpGet("{contractId}/has-been-renewed")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> HasContractBeenRenewed(long contractId)
        {
            if (contractId <= 0)
            {
                return this.BadRequest("Valid contractId is required.");
            }

            try
            {
                bool hasBeenRenewed = await this.contractRenewalRepository.HasContractBeenRenewedAsync(contractId);
                return this.Ok(hasBeenRenewed);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while checking renewal status for contract ID: {contractId}. Error: {ex.Message}");
            }
        }
    }
}
