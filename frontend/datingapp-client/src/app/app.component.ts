import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent implements OnInit {
  title = 'datingapp-client';
  users: any;
  constructor(private http: HttpClient) {}
  ngOnInit(): void {
    console.log('fetching users');
    this.http.get('https://localhost:5001/api/users').subscribe(
      (resp) => {
        this.users = resp;
      },
      (error) => {
        console.log(error);
      }
    );
  }
}
