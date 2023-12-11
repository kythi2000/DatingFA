import { Component, OnInit } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { Observable, of } from 'rxjs';
import { User } from '../_models/user';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit{
  model: any = {}
  currentUser$: Observable<User | null> = of(null);

  constructor(public accountService: AccountService, 
              private router: Router) {}

  ngOnInit(): void{
  }

  login() {
    this.accountService.login(this.model).subscribe({
      next: () => this.router.navigateByUrl('/members'),
      complete: () => console.log(this.accountService.currentUser$)
    });
    this.model = {};
  }

  logout(){
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }
}
