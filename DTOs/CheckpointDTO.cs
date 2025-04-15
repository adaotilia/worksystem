﻿using System;
using worksystem.Models;

namespace worksystem.DTOs
{
    public class CheckpointDTO
    { 
        public int CheckpointId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public required SessionStatus SessionStatus { get; set; }
    }
}