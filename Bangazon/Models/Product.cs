using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bangazon.Models
{
	public class Product
	{
		[Key]
		public int ProductId { get; set; }

		[Required]
		[DataType(DataType.Date)]
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public DateTime DateCreated { get; set; }

		[Required]
		[StringLength(255)]
		public string Description { get; set; }

		[Required]
		[StringLength(55, ErrorMessage = "Please shorten the product title to 55 characters")]
		public string Title { get; set; }

		[Required]
		[DisplayFormat(DataFormatString = "{0:C}")]
		public double Price { get; set; }

		[Required]
        [Range(1, int.MaxValue, ErrorMessage = "Item quantity must be more than 0")]
        public int Quantity { get; set; }

		[Required]
		public string UserId { get; set; }

		[Required]
		[StringLength(55, ErrorMessage = "Please set a city")]
		public string City { get; set; }

		[Required]
		[StringLength(55, ErrorMessage = "Please link an image")]
		public string ImagePath { get; set; }


		[Required]
		public ApplicationUser User { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please choose a Product Category")]
        [Display(Name="Product Category")]
        public int ProductTypeId { get; set; }

		public ProductType ProductType { get; set; }

		public virtual ICollection<OrderProduct> OrderProducts { get; set; }

	}
}
