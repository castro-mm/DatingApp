import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { HttpClient } from '@angular/common/http';
import { User } from '../_models/user';
import { Photo } from '../_models/photo';

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

    getPhotosForApproval() {
        return this.http.get<Photo[]>(this.baseApiUrl + 'admin/photos-to-moderate');
    }

    approvePhoto(photoId: number) {
        return this.http.put(this.baseApiUrl + 'admin/approve-photo/' + photoId, {});
    }

    rejectPhoto(photoId: number) {
        return this.http.delete(this.baseApiUrl + 'admin/delete-photo/' + photoId);
    }
}
