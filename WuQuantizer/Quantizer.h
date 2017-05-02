#pragma once

#ifndef QUANTIZER_H
#define QUANTIZER_H
//struct with RGB and vol values
struct box;

//vars
float m2[33][33][33];
long int wt[33][33][33], mr[33][33][33], mg[33][33][33], mb[33][33][33];
unsigned char *Ir, *Ig, *Ib;
int size, K;
unsigned short int *Qadd;

//Function headers
void Hist3d(int *vwt, int *vmr, int *vmg, int *vmb, float *m2);
void M3d(int *vwt, int *vmr, int *vmg, int *vmb, float *m2);
long int Vol(struct box *cube, int *mmt);
long int Bottom(struct box *cube, unsigned char dir, int *mmt);
long int Top(struct box *cube, unsigned char dir, int pos, int *mmt);
float Var(struct box *cube);
float Maximize(struct box *cube, unsigned char dir,
	int first, int last, int *cut,
	long int whole_r, long int whole_g, long int whole_b, long int whole_w);
int Cut(struct box *set1, struct box *set2);
void Mark(struct box *cube, int label, unsigned char *tag);
#endif
