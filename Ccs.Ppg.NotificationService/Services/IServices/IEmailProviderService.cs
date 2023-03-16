﻿using Ccs.Ppg.NotificationService.Model;

namespace Ccs.Ppg.NotificationService.Services.IServices
{
  public interface IEmailProviderService
  {
    Task SendEmailAsync(EmailInfo emailInfo);
  }
}