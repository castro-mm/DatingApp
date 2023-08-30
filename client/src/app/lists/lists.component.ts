import { Component, OnInit } from '@angular/core';
import { Member } from '../_models/member';
import { MembersService } from '../_services/members.service';
import { LikeParams } from '../_models/likeParams';
import { Pagination } from '../_models/pagination';

@Component({
    selector: 'app-lists',
    templateUrl: './lists.component.html',
    styleUrls: ['./lists.component.css']
})

export class ListsComponent implements OnInit {

    members: Member[] | undefined;
    predicate: string = 'liked';
    likeParams: LikeParams;
    pagination: Pagination | undefined;


    constructor(private memberService: MembersService) {
        this.likeParams = new LikeParams();
        this.likeParams.predicate = this.predicate;        
    }
    ngOnInit(): void {
        this.loadLikes();
    }

    loadLikes() {

        this.likeParams.predicate = this.predicate;        

        this.memberService.getLikes(this.likeParams).subscribe({
            next: response => {
                this.members = response.result;
                this.pagination = response.pagination;
            }
        })
    }

    pageChanged(event: any) {

        if(this.likeParams) {
            if (this.likeParams.pageNumber !== event.page) {
                this.likeParams.pageNumber = event.page;
                this.loadLikes();    
            }     
        }
    }}
