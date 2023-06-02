using Dapper;
using Data.Models;
using Data.Repositories.Interfaces;
using Data.ViewModels;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace Data.Repositories
{
    public class LoanRepository : ILoanRepository
    {
        private readonly string connectionString;

        public LoanRepository(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<int> CreateLoanAsync(Loan loan)
        {
            using var connection = new SqlConnection(connectionString);
            string createLoanSQL = "INSERT INTO Loans (itemId, orderedBy, quantity, loanStartDate, loanEndDate) VALUES (@ItemId, @OrderedBy, @Quantity, @LoanStartDate, @LoanEndDate)";

            int changesByCreatingLoan = await connection.ExecuteAsync(createLoanSQL, loan);

            return changesByCreatingLoan;
        }

        public async Task<Loan[]> GetAllLoansAsync()
        {
            using var connection = new SqlConnection(connectionString);

            var allLoansSQL = "SELECT * FROM Loans";
            var allLoans = await connection.QueryAsync<Loan>(allLoansSQL);

            return allLoans.ToArray();
        }

        public async Task<Loan[]> GetLoansByUserEmailAsync(string userEmail)
        {
            using var connection = new SqlConnection(connectionString);

            var allLoansByUserEmailSQL = "SELECT * FROM Loans WHERE orderedBy = @UserEmail";
            var loans = await connection.QueryAsync<Loan>(allLoansByUserEmailSQL, new { UserEmail = userEmail });

            return loans.ToArray();
        }

        public async Task<int> ReturnLoanAsync(int loanId)
        {
            using var connection = new SqlConnection(connectionString);

            var returnLoanSQL = "UPDATE Loans SET loanEndDate = @LoanEndDate WHERE Id = @Id";
            return await connection.ExecuteAsync(returnLoanSQL, new { LoanEndDate = DateTime.Now ,Id = loanId });
        }
    }
}
