package com.codeReview.socialmedia.repository;

import com.codeReview.socialmedia.model.Post;
//import com.codeReview.socialmedia.model.User;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.mongodb.repository.MongoRepository;
import org.springframework.stereotype.Repository;

import java.util.List;

@Repository
public interface PostRepository extends MongoRepository<Post, String> {

}

//public interface PostRepository{}
