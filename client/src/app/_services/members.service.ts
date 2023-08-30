import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';
import { map, of, take } from 'rxjs';
import { PaginatedResult } from '../_models/pagination';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';
import { User } from '../_models/user';
import { BaseParams } from '../_models/baseParams';
import { LikeParams } from '../_models/likeParams';

@Injectable({
    providedIn: 'root'
})
export class MembersService {

    baseApiUrl = environment.apiUrl;
    members: Member[] = [];
    memberCache = new Map();
    user: User | undefined;
    userParams: UserParams | undefined;

    constructor(private http: HttpClient, private accountService: AccountService) { 

        this.accountService.currentUser$.pipe(take(1)).subscribe({
            next: (user) => {
                if (user) {
                    this.userParams = new UserParams(user);
                    this.user = user;    
                }
            }
        })
    }

    getUserParams() {
        return this.userParams;
    }

    setUserParams(userParams: UserParams) {
        this.userParams = userParams;
    }

    resetUserParams() {
        if(this.user) {
            this.userParams = new UserParams(this.user);
            return this.userParams;
        }
        return;
    }

    getMembers(userParams: UserParams) {

        const response = this.memberCache.get(Object.values(userParams).join('-'));

        if (response) return of(response);

        let params = this.getPaginationHeaders(userParams);
      
        return this.getPaginatedResult<Member[]>(this.baseApiUrl + 'users/', params).pipe(
            map(response => {
                this.memberCache.set(Object.values(userParams).join('-'), response);
                return response;
            })
        );
    }    

    getMember(username: string) {
        const member = [...this.memberCache.values()]
            .reduce((arr, elem) => arr.concat(elem.result), [])
            .find((member: Member) => member.userName === username);

        if(member) return of(member);
        
        return this.http.get<Member>(this.baseApiUrl + 'users/' + username);
    }

    updateMember(member: Member) {
        return this.http.put(this.baseApiUrl + 'users', member).pipe(
            map(() => {
                const index = this.members.indexOf(member);
                this.members[index] = {...this.members[index], ...member};
            })
        );
    }

    setMainPhoto(photoId: number) { 
        return this.http.put(this.baseApiUrl + 'users/set-main-photo/' + photoId, {});
    }

    deletePhoto(photoId: number) {
        return this.http.delete(this.baseApiUrl + 'users/delete-photo/' + photoId);
    }

    addLike(username: string) {
        return this.http.post(this.baseApiUrl + 'likes/' + username, {});
    }

    getLikes(likeParams: LikeParams) {

        let params = this.getPaginationHeaders(likeParams);
      
        return this.getPaginatedResult<Member[]>(this.baseApiUrl + 'likes/', params);

        //return this.http.get<Member[]>(this.baseApiUrl + 'likes/?predicate=' + predicate);
    }

    private getPaginatedResult<T>(url: string, params: HttpParams) {

        const paginatedResult: PaginatedResult<T> = new PaginatedResult<T>();
        return this.http.get<T>(url, { observe: 'response', params }).pipe(
            map((response) => {
                if (response.body) {
                    paginatedResult.result = response.body;
                }
                const pagination = response.headers.get('Pagination');
                if (pagination) {
                    paginatedResult.pagination = JSON.parse(pagination);
                }
                return paginatedResult;
            })
        );
    }

    private getPaginationHeaders(params: any): HttpParams {

        return new HttpParams({fromObject: {...params}});       
    }
}
