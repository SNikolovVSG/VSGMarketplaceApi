using Data.Models;
using Data.ViewModels;

namespace Services.Interfaces
{
    public interface ILoansService
    {
        public Task LoanAsync(int itemId, CreateLoanViewModel input);

        public Task<Loan[]> GetMyLoansAsync(string userId);

        public Task<List<KeyValuePair<string, int>>> GetAllLoansAsync();

        public Task ReturnLoan(int loanId);
    }
}
