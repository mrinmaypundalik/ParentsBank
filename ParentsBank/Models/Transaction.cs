using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ParentsBank.Models
{
    public class Transaction
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int AccountID { get; set; }

        public virtual Account Account { get; set; }

        [Display(Name = "Transaction Date")]
        [CustomValidation(typeof(Transaction), "TransactionDateValidation")]
        public DateTime TransactionDate { get; set; }

        public float Amount { get; set; }

        [Required]
        public string Note { get; set; }

       public Transaction()
        {
            TransactionDate = DateTime.Now;
        }

        public static ValidationResult TransactionDateValidation(DateTime transactionDate, ValidationContext context)
        {
            ValidationResult validationResult;
            if (DateTime.Now < transactionDate)
            {
                validationResult = new ValidationResult("Transaction Date cannot be in the future");
            }
            else if (transactionDate.Year < DateTime.Now.Year)
            {
                validationResult = new ValidationResult("Transactions should belong to current year");
            }
            else
            {
                validationResult = ValidationResult.Success;
            }
            return validationResult;
        }

        public static ValidationResult TransactionAmountValidation(float amount, ValidationContext context)
        {
            ValidationResult validationResult;
            if (amount == 0.0)
            {
                validationResult = new ValidationResult("Transaction Amount cannot be 0");
            }
            else
            {
                validationResult = ValidationResult.Success;
            }
            return validationResult;
        }

    }
}
