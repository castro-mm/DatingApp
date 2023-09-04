import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { HttpClient } from '@angular/common/http';
import { User } from '../_models/user';

@Injectable({
    providedIn: 'root'
})
export class AdminService {
    baseApiUrl: string = environment.apiUrl;

    constructor(private http: HttpClient) { }

    getUsersWithRoles() {
        return this.http.get<User[]>(this.baseApiUrl + 'admin/users-with-roles');
    }

    updateUserRoles(username: string, roles: string[]) {
        return this.http.post<string[]>(this.baseApiUrl + 'admin/edit-roles/' + username + '?roles=' + roles, {});
    }
}
