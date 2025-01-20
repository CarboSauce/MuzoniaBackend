# Muzonia Backend

Backend written in C# for Muzonia music streaming service

## Features

### User

- [x] Register
- [x] Login
- [x] Logout
- [ ] Reset password
- [ ] Change email
- [x] Update Profile
- [x] Delete Profile
- [x] Get My Profile
- [x] Get User By Id
- [x] Get User By Username

### Album

- [x] Create Album
- [x] Update Album
- [x] Delete Album
- [x] Get Album By Id
- [x] Edit album
- [x] Search by name

### Artist

- [x] Create Artist
- [x] Update Artist
- [x] Delete Artist
- [x] Get Artist By Id
- [x] Search by name
- [x] Get My artists
- [x] Get artist albums
- [x] Get artist songs

### Track

- [x] Create Track
- [x] Update Track
- [x] Delete Track
- [x] Search by name
- [x] Get Track By Id
- [x] Get My Tracks
- [x] Get Track By Artist
- [x] Get Track By Album

### History

- [ ] Get My History
- [ ] Clear history
- [ ] Clear history entry by id

### Playlists

- [x] Create Playlist
- [x] Update Playlist
- [x] Delete Playlist
- [x] Search by name
- [x] Get Playlist By Id
- [x] Get My Playlists
- [x] Get Playlist By User
- [x] Add track to playlist
- [x] Remove track from playlist
- [x] Reorder tracks

### Queue

- [ ] Add track to queue
- [ ] Remove track from queue
- [ ] Clear queue
- [ ] Reorder queue
- [ ] Get queue

### Playback

- [ ] Play
- [ ] Pause
- [ ] Skip
- [ ] Seek
- [ ] Repeat
- [ ] Shuffle
- [ ] Volume
- [ ] Get state
- [ ] Get current track
- [ ] Get current queue

### Playback sessions

Sessions are multiuser playback sessions, so all functionality from playback should also apply.
But the functionality needs to be probably replicated due to permission system that doesnt apply to normal per user playback sessions.
Unless playback is flexible enough to account for permission system, alternatively one can just make a playback service to reuse functionality but that adds burden to the frontend.


- [ ] Create session
- [ ] Update session
- [ ] Join session
- [ ] Leave session
- [ ] Delete session
- [ ] Get my sessions
- [ ] Get session by id
- [ ] Change permission
- [ ] Get session members
