﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace ODataBatching8.Models
{
    public partial class UsersInGroup
    {
        [Key]
        public Guid UserId { get; set; }
        [Key]
        public Guid GroupId { get; set; }

        [ForeignKey(nameof(GroupId))]
        [InverseProperty(nameof(Groups.UsersInGroup))]
        public virtual Groups Group { get; set; }
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(Users.UsersInGroup))]
        public virtual Users User { get; set; }
    }
}