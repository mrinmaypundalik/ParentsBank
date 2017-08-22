using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ParentsBank.Models
{
    public class Account
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        [EmailAddress]
        public String Recipient { get; set; }

        [EmailAddress]
        public string Owner { get; set; }

        public string Name { get; set; }

        [Display(Name="Open Date")]
        public DateTime OpenDate { get; set; }

        public float Balance { get; set; }

        [Required]
        public string Description { get; set; }

        public string Username { get; set; }
        public virtual List<Transaction> Transactions { get; set; }
        public virtual List<WishList> WishListItems { get; set; }

        [CustomValidation(typeof(Account), "InterestRateValidation")]
        public float Interest { get; set; }

        public DateTime? InterestAddedDate { get; set; }

        public double? InterestEarned { get; set; }

        public Account()
        {
            OpenDate = DateTime.Now;
        }

        public float YearToDateInterest()
        {
            int CurrentDay = DateTime.Now.DayOfYear;
            bool IsLeapYear = DateTime.IsLeapYear(DateTime.Now.Year);
            int TimePeriod = IsLeapYear ? 366 : 365;
            double amount = Balance;
            double InterestEarned = this.InterestEarned.HasValue ? (Double)this.InterestEarned : 0.0f;
            ApplicationDbContext db = new ApplicationDbContext();
            Account account = db.Accounts.Find(this.ID);

            if (!this.InterestAddedDate.HasValue)
            {
                int duration = CurrentDay - this.OpenDate.DayOfYear;

                amount = amount * Math.Pow((1 + this.Interest / (TimePeriod * 100)), duration);
                account.InterestAddedDate = DateTime.Now;
                account.InterestEarned = amount - Balance;
                account.Balance = (float)amount;
                db.Entry(account).State = EntityState.Modified;
                db.SaveChanges();
                return (float)InterestEarned;

            }

            if (this.InterestAddedDate.Value.DayOfYear < CurrentDay)
            {

                int InterestAddedDate = this.InterestAddedDate.Value.DayOfYear;
                int duration = CurrentDay - InterestAddedDate;

                List<int> dates = new List<int>();
                List<double> amounts = new List<double>();

                if (this.Transactions.Count > 0)
                {
                    foreach (Transaction transaction in Transactions)
                    {
                        dates.Add(transaction.TransactionDate.DayOfYear);
                        amounts.Add(transaction.Amount);
                    }
                }

                for (int day = 0; day < duration; day++)
                {

                    double TodaysAmount = 0.0;

                    if (dates.Count != 0 && dates.Contains(InterestAddedDate + day))
                    {
                        TodaysAmount = amounts.ElementAt(day);

                    }
                    amount = amount + TodaysAmount + InterestEarned;
                    //InterestEarned = InterestEarned + CompoundInterest(amount, this.InterestRate / 100, TimePeriod, day);
                    amount = amount * Math.Pow((1 + this.Interest / (TimePeriod * 100)), 1);
                }
                account.InterestAddedDate = DateTime.Now;
                account.InterestEarned = amount - Balance;
                account.Balance = (float)amount;
                db.Entry(account).State = EntityState.Modified;
                db.SaveChanges();
            }
            //account.InterestEarned = InterestEarned;
            return (float)InterestEarned;
        }



        public static ValidationResult InterestRateValidation(float Interest,ValidationContext context)
        {
            ValidationResult validationResult;
            if (Interest > 0 && Interest < 100)
                validationResult = ValidationResult.Success;
            else if(Interest>=100)
                validationResult = new ValidationResult("Interest Cannot be 100 Or More");
            else
                validationResult = new ValidationResult("Interest Cannot be 0 or Negative");
            return validationResult;
        }
    }
}