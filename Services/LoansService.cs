using Data.Models;
using Data.Repositories.Interfaces;
using Data.ViewModels;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Services.Interfaces;
using System.Linq;
using System.Transactions;

namespace Services
{
    public class LoansService : ILoansService
    {
        private readonly ILoanRepository loanRepository;
        private readonly IItemRepository itemRepository;
        private readonly IValidator<Loan> validator;

        private HttpContextAccessor httpContextAccessor;

        public LoansService(ILoanRepository loanRepository, IValidator<Loan> validator, IItemRepository itemRepository)
        {
            this.loanRepository = loanRepository;
            this.validator = validator;
            this.itemRepository = itemRepository;

            this.httpContextAccessor = new HttpContextAccessor();
        }

        public async Task<List<KeyValuePair<string, int>>> GetAllLoansAsync()
        {
            var allLoansArray = await this.loanRepository.GetAllLoansAsync();

            var emailsAndLoans = allLoansArray.GroupBy(x => x.OrderedBy);
            var list = new List<KeyValuePair<string, int>>();

            foreach (var item in emailsAndLoans)
            {
                list.Add(new KeyValuePair<string, int>(item.Key, item.Count()));
            }

            return list;
        }

        public async Task<Loan[]> GetMyLoansAsync(string userEmail)
        {
            return await this.loanRepository.GetLoansByUserEmailAsync(userEmail);
        }

        public async Task LoanAsync(int itemId, CreateLoanViewModel input)
        {
            Loan loan = new Loan
            {
                LoanStartDate = DateTime.Now,
                OrderedBy = input.OrderedBy,
                Quantity = int.Parse(input.Quantity),
                LoanEndDate = null,
                ItemId = itemId,
            };

            var validationResult = validator.Validate(loan);
            if (!validationResult.IsValid) { throw new Exception(Constants.ValidationError); }

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                int changesByCreatingLoan = await this.loanRepository.CreateLoanAsync(loan);

                if (changesByCreatingLoan == 0)
                {
                    throw new Exception("Loan not approved!");
                }

                int changesByReducingAvailableQuantity = await this.itemRepository.ReduceAvailableQuantity(loan);

                if (changesByReducingAvailableQuantity == 0)
                {
                    throw new Exception("Not available quantity!");
                }

                transactionScope.Complete();
                transactionScope.Dispose();
            }
        }

        public async Task ReturnLoan(int loanId)
        {
            int changes = await this.loanRepository.ReturnLoanAsync(loanId);

            if (changes == 0)
            {
                throw new Exception("Zero changes");
            }
        }
    }
}
