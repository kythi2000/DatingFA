import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { map } from 'rxjs';

export const AdminGuard: CanActivateFn = (route, state) => {
  const accountService = inject(AccountService);
  const toast = inject(ToastrService);

  return accountService.currentUser$.pipe(
    map(user => {
      if (!user) return user;
      if (user.roles.includes('Admin') || user.roles.includes('Moderators')) {
        return true;
      } else {
        toast.error('You cannot enter this area');
        return false;
      }
    })
  );
};
