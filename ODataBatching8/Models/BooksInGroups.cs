﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace ODataBatching8.Models
{
    public partial class BooksInGroups
    {
        [Key]
        public Guid BookId { get; set; }
        [Key]
        public Guid GroupId { get; set; }

        [ForeignKey(nameof(BookId))]
        [InverseProperty("BooksInGroups")]
        public virtual Book Book { get; set; }
        [ForeignKey(nameof(GroupId))]
        [InverseProperty(nameof(Groups.BooksInGroups))]
        public virtual Groups Group { get; set; }
    }
}