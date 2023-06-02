using Data.Models;

namespace Data.Repositories.Interfaces
{
    public interface ILoanRepository
    {
        public Task<Loan[]> GetAllLoansAsync();

        public Task<Loan[]> GetLoansByUserEmailAsync(string email);

        public Task<int> ReturnLoanAsync(int loanId);

        public Task<int> CreateLoanAsync(Loan loan);
    }
}
