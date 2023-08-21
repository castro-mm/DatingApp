import { Component, OnInit } from '@angular/core';
import { AccountService } from './_services/account.service';
import { User } from './_models/user';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.css']
})

export class AppComponent implements OnInit {

    title: string = 'Dating App';

    constructor(private accountService: AccountService) {}
    ngOnInit(): void {
        this.setCurrentUser();
    }

    setCurrentUser() {

        const user: User = JSON.parse(localStorage.getItem('user')!);

        // The code above have the same significance of the code below:
        /*
        const userString = localStorage.getItem('user');
        if(!userString) return;
        const userx: User = JSON.parse(userString);
        */

        this.accountService.setCurrentUser(user); // or userx


    }
}
